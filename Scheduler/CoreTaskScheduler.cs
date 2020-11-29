using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Scheduler
{
    public abstract class CoreTaskScheduler : TaskScheduler
    {
        protected ConcurrentQueue<PrioritizedLimitedTask> pendingTasks = new ConcurrentQueue<PrioritizedLimitedTask>();

        protected readonly int maxLevelOfParallelism;

        public override int MaximumConcurrencyLevel => maxLevelOfParallelism;

        /// <summary>
        /// By default, this scheduler works as non-preemtive task scheduler. 
        /// </summary>
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
            tasksForScheduling = tasksForScheduling.OrderBy(x => x.Priority, new PriorityComparer()).ToList();
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

        protected void SortPendingActions()
        {
            pendingTasks = new ConcurrentQueue<PrioritizedLimitedTask>(pendingTasks.AsParallel()
                                 .WithDegreeOfParallelism(Environment.ProcessorCount)
                                 .OrderBy(x => x.Priority, new PriorityComparer()));
        }
    }
}
