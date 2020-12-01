using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
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
            PrioritizedLimitedTask taskToPause = executingTasks.Values
                                                               .Where(x => x.CooperationMechanism.CanBePaused)
                                                               .Min();
            if (taskToPause != null && priority.CompareTo(taskToPause.Priority) > 0)
                RequestContextSwitch(task as PrioritizedLimitedTask, taskToPause);
            else
            {
                SortPendingTasks();
                RunScheduling();
            }
        }

        /// <summary>
        /// Delays execution of a callback for specific amount of time.
        /// </summary>
        /// <param name="taskWithInformation">Task that contains information about that time.</param>
        private void PauseCallback(PrioritizedLimitedTask taskWithInformation)
        {
            do
            {
                if (taskWithInformation.CooperationMechanism.IsPaused || taskWithInformation.CooperationMechanism.IsResumed)
                    Task.Delay(taskWithInformation.CooperationMechanism.PausedFor).Wait();
            }
            while (taskWithInformation.CooperationMechanism.IsPaused);
        }

        /// <summary>
        /// Using preemptive algorithm, runs tasks (number of tasks dependins on parallelism level).
        /// Deadlock avoidance is enabled.
        /// </summary>
        public override void RunScheduling()
        {
            lock (schedulingLocker)
                while (!pendingTasks.IsEmpty && executingTasks.Count < maxLevelOfParallelism)
                {
                    // Get task that is next for execution (task with highest priority)
                    PrioritizedLimitedTask taskWithInformation = GetNextTaskWithDeadlockAvoidance();
                    executingTasks.TryAdd(taskWithInformation.PrioritizedLimitedTaskIdentifier, taskWithInformation);

                    // If pending task was paused, than callback exist and does not need to be created again
                    if (!taskWithInformation.CooperationMechanism.IsPaused && !taskWithInformation.CooperationMechanism.IsResumed)
                        StartCallback(taskWithInformation,
                                  () => executingTasks.Remove(taskWithInformation.PrioritizedLimitedTaskIdentifier, out _),
                                  () => PauseCallback(taskWithInformation));

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

        /// <summary>
        /// Executes specified task and starts cooresponding callback necessary for cooperation.
        /// </summary>
        /// <param name="taskWithInformation">Task to be executed</param>
        private void RunTask(PrioritizedLimitedTask taskWithInformation)
        {
            lock (schedulingLocker)
            {
                executingTasks.TryAdd(taskWithInformation.PrioritizedLimitedTaskIdentifier, taskWithInformation);

                // If pending task was paused, than callback exist and does not need to be started again
                if (!taskWithInformation.CooperationMechanism.IsPaused && !taskWithInformation.CooperationMechanism.IsResumed)
                    StartCallback(taskWithInformation,
                                    () => executingTasks.Remove(taskWithInformation.PrioritizedLimitedTaskIdentifier, out _),
                                    () => PauseCallback(taskWithInformation));

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

        /// <summary>
        /// This method simulates a context switch (pauses one task, and runs another) if there is a guarantee that deadlock won't happen. 
        /// If a task does not get necessary resources, normal scheduling is called (no context switching and no interruption).
        /// </summary>
        /// <param name="taskForExecution">Task to be executed</param>
        /// <param name="taskToPause">Task to pause</param>
        private void RequestContextSwitch(PrioritizedLimitedTask taskForExecution, PrioritizedLimitedTask taskToPause)
        {
            lock (schedulingLocker)
                // TODO: Move
                if (executingTasks.Count >= maxLevelOfParallelism && CanAvoidDeadlock(taskForExecution))
                {
                    executingTasks.Remove(taskToPause.PrioritizedLimitedTaskIdentifier, out _);
                    taskToPause.CooperationMechanism.Pause(taskToPause.DurationInMiliseconds);
                    pendingTasks.Enqueue(taskToPause);
                    RunTask(taskForExecution);
                }
                else RunScheduling();
        }

        /// <summary>
        /// If a task uses no shared resources, deadlock will be avoided. Otherwise, 
        /// call is delegated to a manager that uses Banker's algorithm and decides whether deadlock can be avoided <see cref="IBankerAlgorithm"/>. 
        /// </summary>
        private bool CanAvoidDeadlock(PrioritizedLimitedTask task)
        {
            lock (schedulingLocker)
            {
                RequestApproval approved = RequestApproval.Denied;
                if (task.UsesSharedResources())
                    approved = sharedResourceManager.AllocateResources(task.PrioritizedLimitedTaskIdentifier
                                                                                      , task.SharedResources);
                return approved == RequestApproval.Approved;
            }
        }
    }
}
