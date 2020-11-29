using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Scheduler
{
    public class CoreTaskScheduler : TaskScheduler
    {
        // TODO: Change to private
        public ConcurrentQueue<Task> pendingTasks = new ConcurrentQueue<Task>();
        public ConcurrentDictionary<int, PrioritizedLimitedTask> taskInformation = new ConcurrentDictionary<int, PrioritizedLimitedTask>();

        private readonly bool isPreemtive = false;
        private readonly int maxLevelOfParallelism;

        private int currentlyRunningTasks = 0;
        private object mylocker = new object();

        public override int MaximumConcurrencyLevel => maxLevelOfParallelism;

        /// <summary>
        /// By default, this scheduler works as non-preemtive task scheduler. 
        /// </summary>
        /// <param name="isPreemtive">Send true if  a task scheduler is planned to be used as 
        /// a preemtive task scheduler</param>
        public CoreTaskScheduler(int maxLevelOfParallelism, bool isPreemtive = false)
        {
            this.isPreemtive = isPreemtive;
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
            {
                Task taskForExecution = Task.Factory.StartNew(taskWithInformation.Action, CancellationToken.None, TaskCreationOptions.None, this);
                taskInformation.TryAdd(taskForExecution.Id, taskWithInformation);
                RunScheduling();
            }
            /*  foreach (PrioritizedLimitedTask taskWithInformation in tasksForScheduling)
              {
                  Task taskForExecution = Task.Factory.StartNew(taskWithInformation.Action, CancellationToken.None, TaskCreationOptions.None, this);
                  taskInformation.TryAdd(taskForExecution.Id, taskWithInformation);
                  RunScheduling();
              }*/
        }

        /// <summary>
        /// Schedules task for execution. U sustini ovo radi schedulovanje.
        /// </summary>
        /// <param name="task"></param>
        protected override void QueueTask(Task task)
        {
            pendingTasks.Enqueue(task);
            if (task is PrioritizedLimitedTask)
            {
                taskInformation.TryAdd(task.Id, task as PrioritizedLimitedTask);
                RunScheduling();
            }
            if (isPreemtive)
            {
                //TODO: Run scheduling mechanism
            }
        }

        public void RunScheduling()
        {
            for (int i = 0; i < maxLevelOfParallelism && !pendingTasks.IsEmpty && currentlyRunningTasks < 2; ++i)
            {
                pendingTasks.TryDequeue(out Task taskForExecution);
                PrioritizedLimitedTask taskWithInformation = taskInformation[taskForExecution.Id];
                Task collaborationTask = Task.Factory.StartNew(() =>
                {
                    Console.WriteLine("Started callback for " + taskForExecution.Id);
                    Task.Delay(taskWithInformation.DurationInMiliseconds).Wait();
                    taskWithInformation.cooperationMechanism.Abandone();
                    Console.WriteLine("TASK " + taskWithInformation.Id + " COMPLETED");
                    Interlocked.Decrement(ref currentlyRunningTasks);
                    RunScheduling();
                }, CancellationToken.None, TaskCreationOptions.None, Default);
                Interlocked.Increment(ref currentlyRunningTasks);
                new Task(() => TryExecuteTask(taskForExecution)).Start();
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }


        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return pendingTasks;
        }
        /* private void SortPendingActions()
         {
             var sorted = pendingTasks.AsParallel()
                                  .WithDegreeOfParallelism(Environment.ProcessorCount)
                                  .OrderBy(x => x.Priority, new PriorityComparer());
             pendingTasks = new ConcurrentQueue<PrioritizedLimitedTask>(sorted);
         }*/
    }
}
