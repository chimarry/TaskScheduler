using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Scheduler
{
    /// <summary>
    /// Implements task scheduler <see cref="CoreTaskScheduler"/> as preemptive task scheduler.
    /// </summary>
    public class PreemptiveTaskScheduler : CoreTaskScheduler
    {
        /// <summary>
        /// Collection that stores information about tasks that are currently running.
        /// </summary>
        private readonly ConcurrentDictionary<int, PrioritizedLimitedTask> executingTasks = new ConcurrentDictionary<int, PrioritizedLimitedTask>();

        public override int MaximumConcurrencyLevel => int.MaxValue;

        public PreemptiveTaskScheduler(int maxLevelOfParallelism) : base(maxLevelOfParallelism)
        {
        }

        /// <summary>
        /// Schedules task based on a priority. If the task has a greater prirority than one of the currently running tasks, 
        /// and if one of the currently running tasks allows cooperative context-switching, than context-switching will happen.
        /// </summary>
        /// <param name="task">Task to be scheduled (must extend <see cref="PrioritizedLimitedTask"/>), 
        /// otherwise exception will be thrown</param>
        /// <exception cref="InvalidTaskException"></exception>
        protected override void QueueTask(Task task)
        {
            if (!(task is PrioritizedLimitedTask))
                throw new InvalidTaskException();

            pendingTasks.Enqueue(task as PrioritizedLimitedTask);
            Priority priority = (task as PrioritizedLimitedTask).Priority;
            PriorityComparer priorityComparer = new PriorityComparer();
            PrioritizedLimitedTask taskToPause = executingTasks.Values.Where(x => x.CooperationMechanism.CanBePaused).Min();
            if (taskToPause != null && priority.CompareTo(taskToPause.Priority) > 0 && executingTasks.Count >= maxLevelOfParallelism)
            {
                executingTasks.Remove(taskToPause.PrioritizedLimitedTaskIdentifier, out _);
                taskToPause.CooperationMechanism.Pause((task as PrioritizedLimitedTask).DurationInMiliseconds);
                pendingTasks.Enqueue(taskToPause);
            }
            SortPendingTasks();
            RunScheduling();
        }

        /// <summary>
        /// Using preemptive algorithm, runs task.
        /// </summary>
        public override void RunScheduling()
        {
            lock (schedulingLocker)
                while (!pendingTasks.IsEmpty && executingTasks.Count < maxLevelOfParallelism)
                {
                    // Get task that is next for execution (task with highest priority)
                    PrioritizedLimitedTask taskWithInformation = GetNextTask();
                    executingTasks.TryAdd(taskWithInformation.PrioritizedLimitedTaskIdentifier, taskWithInformation);

                    // If pending task was paused, than callback exist and does not need to be created again
                    if (!taskWithInformation.CooperationMechanism.IsPaused && !taskWithInformation.CooperationMechanism.IsResumed)
                    {
                        Task collaborationTask = Task.Factory.StartNew(() =>
                        {
                            Task.Delay(taskWithInformation.DurationInMiliseconds).Wait();
                            // If task was ever resumed or is currently paused, wait additional time
                            do
                            {
                                if (taskWithInformation.CooperationMechanism.IsPaused || taskWithInformation.CooperationMechanism.IsResumed)
                                    Task.Delay(taskWithInformation.CooperationMechanism.PausedFor).Wait();
                            }
                            while (taskWithInformation.CooperationMechanism.IsPaused);
                            taskWithInformation.CooperationMechanism.Cancel();
                            // Free shared resources
                            if (taskWithInformation.UsesSharedResources())
                                sharedResourceManager.FreeResources(taskWithInformation.PrioritizedLimitedTaskIdentifier, taskWithInformation.SharedResources);
                            executingTasks.Remove(taskWithInformation.PrioritizedLimitedTaskIdentifier, out _);
                            RunScheduling();
                        }, CancellationToken.None, TaskCreationOptions.None, Default);
                    }
                    // Do not block a main thread.
                    new Task(() =>
                    {
                        if (taskWithInformation.CooperationMechanism.IsPaused)
                            taskWithInformation.CooperationMechanism.Resume();
                        else
                            TryExecuteTask(taskWithInformation);
                    }).Start();
                }
        }
    }
}
