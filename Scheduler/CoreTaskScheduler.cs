using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Scheduler
{
    /// <summary>
    /// Abstract class used for priority task scheduling.
    /// </summary>
    public abstract class CoreTaskScheduler : TaskScheduler
    {
        protected readonly IBankerAlgorithm sharedResourceManager = new BankerAlgorithm();

        /// <summary>
        /// Queue containg pending tasks.
        /// </summary>
        protected ConcurrentQueue<PrioritizedLimitedTask> pendingTasks = new ConcurrentQueue<PrioritizedLimitedTask>();

        /// <summary>
        /// Number of user's task that can be run in parallel.
        /// </summary>
        protected readonly int maxLevelOfParallelism;

        /// <summary>
        /// Basic locker.
        /// </summary>
        protected readonly object schedulingLocker = new object();

        public override int MaximumConcurrencyLevel => maxLevelOfParallelism;

        public CoreTaskScheduler(int maxLevelOfParallelism)
        {
            this.maxLevelOfParallelism = maxLevelOfParallelism;
        }

        /// <summary>
        /// Queues multiple actions for scheduling.
        /// </summary>
        /// <param name="tasksForScheduling">List of actions to schedule</param>
        public void QueueForScheduling(IList<PrioritizedLimitedTask> tasksForScheduling)
        {
            tasksForScheduling = tasksForScheduling.OrderByDescending(x => x.Priority, new PriorityComparer()).ToList();
            foreach (PrioritizedLimitedTask taskWithInformation in tasksForScheduling)
                taskWithInformation.Start(this);
        }

        protected abstract override void QueueTask(Task task);

        public abstract void RunScheduling();

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }


        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return pendingTasks;
        }

        /// <summary>
        /// Sorts pending tasks descending by priority <see cref="Priority"/>
        /// </summary>
        protected void SortPendingTasks()
        {
            pendingTasks = new ConcurrentQueue<PrioritizedLimitedTask>(
                     pendingTasks.AsParallel()
                                 .WithDegreeOfParallelism(Environment.ProcessorCount)
                                 .OrderByDescending(x => x.Priority, new PriorityComparer()));
        }

        /// <summary>
        /// Finds and returns a task that has all necessary resources and allocates those resources. 
        /// It assumes that list is not empty, so a caller of this method needs to check that.
        /// </summary>
        protected PrioritizedLimitedTask GetNextTaskWithDeadlockAvoidance()
        {
            pendingTasks.TryDequeue(out PrioritizedLimitedTask taskWithInformation);
            if (taskWithInformation.UsesSharedResources())
            {
                RequestApproval approved = sharedResourceManager.AllocateResources(taskWithInformation.PrioritizedLimitedTaskIdentifier
                                                                                   , taskWithInformation.SharedResources);
                if (approved == RequestApproval.Wait)
                {
                    PrioritizedLimitedTask nextTask = GetNextTaskWithDeadlockAvoidance();
                    pendingTasks.Enqueue(taskWithInformation);
                    taskWithInformation = nextTask;
                }
            }
            return taskWithInformation;
        }

        /// <summary>
        /// Starts (creates and schedules) task that is used to cooperatively cancels (and optionally pauses) user's task. 
        /// Is user's task used resources, resources are released.
        /// This task is scheduled using default .NET scheduler.
        /// </summary>
        /// <param name="task">Corresponding user's task</param>
        /// <param name="controlNumberOfExecutionTasksAction">Responsible for decrementing number of currently running tasks</param>
        /// <param name="enablePauseAction">Responsible for pausing this callback for specific amount of time (optional)</param>
        protected void StartCallback(PrioritizedLimitedTask task, Action controlNumberOfExecutionTasksAction, Action enablePauseAction = null)
            => Task.Factory.StartNew(() =>
                {
                    Task.Delay(task.DurationInMiliseconds).Wait();
                    enablePauseAction?.Invoke();
                    task.CooperationMechanism.Cancel();
                    // Free shared resources
                    if (task.UsesSharedResources())
                        sharedResourceManager.FreeResources(task.PrioritizedLimitedTaskIdentifier, task.SharedResources);
                    controlNumberOfExecutionTasksAction.Invoke();
                    RunScheduling();
                }, CancellationToken.None, TaskCreationOptions.None, Default);
    }
}
