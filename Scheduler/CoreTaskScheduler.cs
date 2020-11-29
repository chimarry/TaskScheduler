using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scheduler
{
    /// <summary>
    /// Abstract class used for priority task scheduling.
    /// </summary>
    public abstract class CoreTaskScheduler : TaskScheduler
    {
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
        /// Sorts pending tasks descending by priority.
        /// </summary>
        protected void SortPendingTasks()
        {
            pendingTasks = new ConcurrentQueue<PrioritizedLimitedTask>(
                     pendingTasks.AsParallel()
                                 .WithDegreeOfParallelism(Environment.ProcessorCount)
                                 .OrderByDescending(x => x.Priority, new PriorityComparer()));
        }
    }
}
