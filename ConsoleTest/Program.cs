using Scheduler;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace ConsoleTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Scheduler.CoreTaskScheduler coreTaskScheduler = new Scheduler.CoreTaskScheduler(2);
            CooperationMechanism cooperationMechanism1 = new CooperationMechanism();
            void action1()
            {
                while (true)
                {
                    Console.WriteLine("Action 1 is executing");
                    Thread.Sleep(100);
                    if (cooperationMechanism1.IsAbandoned)
                        break;
                }
            }
            CooperationMechanism cooperationMechanism2 = new CooperationMechanism();
            void action2()
            {
                while (true)
                {
                    Console.WriteLine("Action 2 is executing");
                    Thread.Sleep(100);
                    if (cooperationMechanism2.IsAbandoned)
                        break;
                }
            }

            CooperationMechanism cooperationMechanism3 = new CooperationMechanism();
            void action3()
            {
                while (true)
                {
                    Console.WriteLine("Action 3 is executing");
                    Thread.Sleep(100);
                    if (cooperationMechanism3.IsAbandoned)
                        break;
                }
            }

            coreTaskScheduler.QueueForScheduling(new List<PrioritizedLimitedTask>()
            {
                new PrioritizedLimitedTask(action1, Priority.Lowest, 2000)
                {
                    cooperationMechanism = cooperationMechanism1
                },
                new PrioritizedLimitedTask(action2, Priority.High, 1000)
                {
                    cooperationMechanism = cooperationMechanism2
                },
                 new PrioritizedLimitedTask(action3, Priority.Normal, 1300)
                 {
                     cooperationMechanism = cooperationMechanism3
                 }
            });
            /* Thread.Sleep(1000);
             foreach (PrioritizedLimitedTask task in coreTaskScheduler.pendingTasks)
                 Console.WriteLine(task.Priority + " " + task.ActionIdentifier);*/
            coreTaskScheduler.Schedule();
            Task task = new Task(() =>
            {
                Task.Delay(3300).Wait();
                Console.WriteLine(cooperationMechanism1.IsAbandoned + "" + cooperationMechanism2.IsAbandoned + cooperationMechanism3.IsAbandoned + "");
            });
            task.Start();
            Task.WaitAll(task);
        }
    }
}
