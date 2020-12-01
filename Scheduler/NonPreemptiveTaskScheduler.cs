using System.Threading;
using System.Threading.Tasks;

namespace Scheduler
{
    /// <summary>
    /// Implements task scheduler <see cref="CoreTaskScheduler"/> as non-preemptive task scheduler.
    /// </summary>
    public class NonPreemptiveTaskScheduler : CoreTaskScheduler
    {
        /// <summary>
        /// Number of currently running tasks.
        /// </summary>
        private int currentlyRunningTasks = 0;

        public NonPreemptiveTaskScheduler(int maxLevelOfParallelism) : base(maxLevelOfParallelism) { }

        /// <summary>
        /// Schedules task based on priority and without interrupting currently running tasks.
        /// </summary>
        /// <param name="task">Task to be scheduled (must extend <see cref="PrioritizedLimitedTask"/>), 
        /// otherwise exception will be thrown</param>
        /// <exception cref="InvalidTaskException"></exception>
        protected override void QueueTask(Task task)
        {
            if (!(task is PrioritizedLimitedTask))
                throw new InvalidTaskException();

            pendingTasks.Enqueue(task as PrioritizedLimitedTask);
            SortPendingTasks();
            RunScheduling();
        }

        /// <summary>
        /// Using preemptive scheduling algorithm, runs task.
        /// </summary>
        public override void RunScheduling()
        {
            lock (schedulingLocker)
            {
                while (!pendingTasks.IsEmpty && currentlyRunningTasks < maxLevelOfParallelism)
                {
                    PrioritizedLimitedTask taskWithInformation = GetNextTaskWithDeadlockAvoidance();
                    StartCallback(taskWithInformation, () => Interlocked.Decrement(ref currentlyRunningTasks));
                    Interlocked.Increment(ref currentlyRunningTasks);

                    // Do not block a main thread
                    new Task(() => TryExecuteTask(taskWithInformation)).Start();
                }
            }
        }
    }
}
