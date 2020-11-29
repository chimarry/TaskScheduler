using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Scheduler
{
    public class PreemtiveTaskScheduler : CoreTaskScheduler
    {
        private List<PrioritizedLimitedTask> executingTasks = new List<PrioritizedLimitedTask>();
        public override int MaximumConcurrencyLevel => int.MaxValue;

        public PreemtiveTaskScheduler(int maxLevelOfParallelism) : base(maxLevelOfParallelism)
        {
        }

        public override void RunScheduling()
        {
            while (!pendingTasks.IsEmpty && executingTasks.Count < maxLevelOfParallelism)
            {
                pendingTasks.TryDequeue(out PrioritizedLimitedTask taskWithInformation);

                executingTasks.Add(taskWithInformation);

                if (!taskWithInformation.CooperationMechanism.IsPaused && !taskWithInformation.CooperationMechanism.IsResumed)
                {
                    Task collaborationTask = Task.Factory.StartNew(() =>
                    {
                        Task.Delay(taskWithInformation.DurationInMiliseconds).Wait();
                        if (taskWithInformation.CooperationMechanism.IsPaused || taskWithInformation.CooperationMechanism.IsResumed)
                            Task.Delay(taskWithInformation.CooperationMechanism.PausedFor).Wait();

                        taskWithInformation.CooperationMechanism.Cancel();
                        executingTasks.Remove(taskWithInformation);
                        RunScheduling();
                    }, CancellationToken.None, TaskCreationOptions.None, Default);
                }
                new Task(() =>
                {
                    if (taskWithInformation.CooperationMechanism.IsPaused)
                        taskWithInformation.CooperationMechanism.Resume();
                    else
                        TryExecuteTask(taskWithInformation);
                }).Start();
            }
        }

        protected override void QueueTask(Task task)
        {

            if (!(task is PrioritizedLimitedTask))
                throw new InvalidTaskException();

            pendingTasks.Enqueue(task as PrioritizedLimitedTask);
            Priority priority = (task as PrioritizedLimitedTask).Priority;
            PriorityComparer priorityComparer = new PriorityComparer();
            PrioritizedLimitedTask taskToPause = executingTasks.Min();
            if (taskToPause != null && priority.CompareTo(taskToPause.Priority) > 0 && executingTasks.Count >= maxLevelOfParallelism)
            {
                executingTasks.Remove(taskToPause);
                taskToPause.CooperationMechanism.Pause((task as PrioritizedLimitedTask).DurationInMiliseconds);
                pendingTasks.Enqueue(taskToPause);
            }
            SortPendingActions();
            RunScheduling();
        }
    }
}
