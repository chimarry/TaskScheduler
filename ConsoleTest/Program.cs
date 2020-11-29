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
            NonPreemtiveTaskScheduler coreTaskScheduler = new NonPreemtiveTaskScheduler(2);
            CooperationMechanism cooperationMechanism1 = new CooperationMechanism();
            void action1()
            {
                while (true)
                {
                    Console.WriteLine("Action 1 is executing");
                    Thread.Sleep(100);
                    if (cooperationMechanism1.IsCancelled)
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
                    if (cooperationMechanism2.IsCancelled)
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
                    if (cooperationMechanism3.IsCancelled)
                        break;
                }
            }


            CooperationMechanism cooperationMechanism4 = new CooperationMechanism();
            void action4()
            {
                while (true)
                {
                    Console.WriteLine("Action 4 is executing");
                    Thread.Sleep(150);
                    if (cooperationMechanism4.IsCancelled)
                        break;
                }
            }

            coreTaskScheduler.QueueForScheduling(new List<PrioritizedLimitedTask>()
            {
                new PrioritizedLimitedTask(action1, Priority.Lowest, 2000)
                {
                    cooperationMechanism = cooperationMechanism1
                },
                new PrioritizedLimitedTask(action2, Priority.High, 2200)
                {
                    cooperationMechanism = cooperationMechanism2
                },
                 new PrioritizedLimitedTask(action3, Priority.Normal, 2000)
                 {
                     cooperationMechanism = cooperationMechanism3
                 }
            });

            PrioritizedLimitedTask prioritizedLimitedTask = new PrioritizedLimitedTask(action4, Priority.Highest, 500)
            {
                cooperationMechanism = cooperationMechanism4
            };
            prioritizedLimitedTask.Start(coreTaskScheduler);
            Task task = new Task(() =>
            {
                Task.Delay(3300).Wait();
                Console.WriteLine(cooperationMechanism1.IsCancelled + "" + cooperationMechanism2.IsCancelled + cooperationMechanism3.IsCancelled + "");
            });
            task.Start();
            Task.WaitAll(task);
        }
    }
}
