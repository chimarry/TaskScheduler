using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Scheduler
{
    public class CoreTaskScheduler : TaskScheduler, IRealTimeScheduler
    {
        public ConcurrentQueue<PrioritizedLimitedTask> pendingTasks = new ConcurrentQueue<PrioritizedLimitedTask>();
        private List<(PrioritizedLimitedTask, Task)> scheduledTasks = new List<(PrioritizedLimitedTask, Task)>();
        private object pendingTasksLocker = new object();
        private readonly bool isPreemtive = false;
        private readonly int maxLevelOfParallelism;


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
        /// <param name="actions">List of actions to schedule</param>
        public void QueueForScheduling(IList<PrioritizedLimitedTask> actions)
        {
            lock (pendingTasksLocker)
            {
                foreach (PrioritizedLimitedTask action in actions)
                    pendingTasks.Enqueue(action);
                SortPendingActions();
            }
        }

        /// <summary>
        /// Enables adding action to a queue of pending actions for scheduling in a real-time.
        /// Scheduling will be called immediately if task scheduler is used as preemtive task scheduler.
        /// </summary>
        /// <param name="prioritizedLimitedTask">The action to be scheduled</param>
        public void QueueForScheduling(PrioritizedLimitedTask prioritizedLimitedTask)
        {
            lock (pendingTasksLocker)
            {
                QueueTask(prioritizedLimitedTask);
            }
        }

        public void ScheduleTasks()
        {
            /* moze da se izvrsava samo n taskova paralelno
             * mislim da cu da uradim bez context switchinga
             * poziva se kada dodje task sa vecim prioritetom ili kada neki task zavrsi sa poslom
             kada dodje task sa vecim prioritetom, trebao bi da se taj task koji je prekinut privremeno pauzira, pa onda nastavi,
             * nekako koristeci asinhrono programiranje -> tipa: pokrene se task, ima mogucnost da se otkaze nasilno ili obicno. 
             * kada se otkaze, on se ne zaustavlja, vec ceka da dobije mogucnost da se opet pokrene, pa se onda opet pokrene
             * paziti na dijeljene resurse -> nekako komunicirati sa korisnickom funkcijom povodom dijeljenih resursa
             */
            // 
        }

        public void Schedule()
        {
            lock (pendingTasksLocker)
            {
                for (int i = 0; i < maxLevelOfParallelism && !pendingTasks.IsEmpty; ++i)
                {
                    pendingTasks.TryDequeue(out PrioritizedLimitedTask executingTask);
                    // TODO: check for shared resources -> ako ima sve resurse, nastavi izvrsavanje, ako nema, neka ceka ponovo
                    Task callableTask = new Task(() =>
                       {
                           Task.Delay(executingTask.DurationInMiliseconds).Wait();
                           executingTask.cooperationMechanism.Abandone();
                           Schedule();
                       });
                    executingTask.Start();
                    callableTask.Start();
                    Console.WriteLine("Started " + i + " " + maxLevelOfParallelism);
                    scheduledTasks.Add((executingTask, callableTask));
                }
            }
        }

        protected override IEnumerable<Task> GetScheduledTasks()
        {
            return scheduledTasks.Select(x => x.Item1);
        }

        protected override void QueueTask(Task task)
        {
            lock (pendingTasksLocker)
            {
                if (!(task is PrioritizedLimitedTask))
                    throw new InvalidTaskException();

                pendingTasks.Enqueue(task as PrioritizedLimitedTask);
                SortPendingActions();
                if (isPreemtive)
                {
                    //TODO: Run scheduling mechanism
                }
            }
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            return false;
        }

        private void SortPendingActions()
        {
            var sorted = pendingTasks.AsParallel()
                                 .WithDegreeOfParallelism(Environment.ProcessorCount)
                                 .OrderBy(x => x.Priority, new PriorityComparer());
            pendingTasks = new ConcurrentQueue<PrioritizedLimitedTask>(sorted);
        }
    }
}
