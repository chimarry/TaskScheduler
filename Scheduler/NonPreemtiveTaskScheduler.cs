using System.Threading;
using System.Threading.Tasks;

namespace Scheduler
{
    public class NonPreemtiveTaskScheduler : CoreTaskScheduler
    {
        private readonly object schedulingLocker = new object();
        private int currentlyRunningTasks = 0;

        public NonPreemtiveTaskScheduler(int maxLevelOfParallelism) : base(maxLevelOfParallelism)
        {
        }

        protected override void QueueTask(Task task)
        {
            if (!(task is PrioritizedLimitedTask))
                throw new InvalidTaskException();

            pendingTasks.Enqueue(task as PrioritizedLimitedTask);
            SortPendingActions();
            RunScheduling();
        }

        public override void RunScheduling()
        {
            lock (schedulingLocker)
            {
                for (int i = 0; i < maxLevelOfParallelism && !pendingTasks.IsEmpty && currentlyRunningTasks < 2; ++i)
                {
                    pendingTasks.TryDequeue(out PrioritizedLimitedTask taskWithInformation);
                    // Using default task scheduler, enable cooperative execution
                    Task collaborationTask = Task.Factory.StartNew(() =>
                    {
                        Task.Delay(taskWithInformation.DurationInMiliseconds).Wait();
                        taskWithInformation.CooperationMechanism.Cancel();
                        Interlocked.Decrement(ref currentlyRunningTasks);
                        RunScheduling();
                    }, CancellationToken.None, TaskCreationOptions.None, Default);

                    Interlocked.Increment(ref currentlyRunningTasks);
                    new Task(() => TryExecuteTask(taskWithInformation)).Start();
                }
            }
        }
    }
}
