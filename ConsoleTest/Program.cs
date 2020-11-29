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
            /* NonPreemtiveTaskScheduler coreTaskScheduler = new NonPreemtiveTaskScheduler(2);
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
                     CooperationMechanism = cooperationMechanism1
                 },
                 new PrioritizedLimitedTask(action2, Priority.High, 2200)
                 {
                     CooperationMechanism = cooperationMechanism2
                 },
                  new PrioritizedLimitedTask(action3, Priority.Normal, 2000)
                  {
                      CooperationMechanism = cooperationMechanism3
                  }
             });

             PrioritizedLimitedTask prioritizedLimitedTask = new PrioritizedLimitedTask(action4, Priority.Highest, 500)
             {
                 CooperationMechanism = cooperationMechanism4
             };
             prioritizedLimitedTask.Start(coreTaskScheduler);
             Task task = new Task(() =>
             {
                 Task.Delay(3300).Wait();
                 Console.WriteLine(cooperationMechanism1.IsCancelled + "" + cooperationMechanism2.IsCancelled + cooperationMechanism3.IsCancelled + "");
             });
             task.Start();
             Task.WaitAll(task);*/
            PreemtiveTaskScheduler coreTaskScheduler = new PreemtiveTaskScheduler(2);
            CooperationMechanism cooperationMechanism1 = new CooperationMechanism();
            void action1()
            {
                while (true)
                {
                    if (cooperationMechanism1.IsCancelled)
                    {
                        Console.WriteLine("Action 1 is cancelled");
                        break;
                    }
                    if (cooperationMechanism1.Paused > 0)
                    {
                        Console.WriteLine("Action 1 is paused");
                        Task.Delay(cooperationMechanism1.Paused).Wait();
                    }
                    Console.WriteLine("Action 1 is executing");
                    Thread.Sleep(100);

                }
            }
            CooperationMechanism cooperationMechanism2 = new CooperationMechanism();
            void action2()
            {
                while (true)
                {
                    if (cooperationMechanism2.IsCancelled)
                    {
                        Console.WriteLine("Action 2 is cancelled");
                        break;
                    }
                    if (cooperationMechanism2.Paused > 0)
                    {
                        Console.WriteLine("Action 2 is paused");
                        Task.Delay(cooperationMechanism2.Paused).Wait();
                    }
                    Console.WriteLine("Action 2 is executing");
                    Thread.Sleep(100);

                }
            }

            CooperationMechanism cooperationMechanism3 = new CooperationMechanism();
            void action3()
            {
                while (true)
                {
                    if (cooperationMechanism3.IsCancelled)
                    {
                        Console.WriteLine("Action 3 is cancelled");
                        break;
                    }
                    if (cooperationMechanism3.Paused > 0)
                    {
                        Console.WriteLine("Action 3 is paused");
                        Task.Delay(cooperationMechanism3.Paused).Wait();
                    }
                    Console.WriteLine("Action 3 is executing");
                    Thread.Sleep(100);

                }
            }


            CooperationMechanism cooperationMechanism4 = new CooperationMechanism();
            void action4()
            {
                while (true)
                {
                    if (cooperationMechanism4.IsCancelled)
                    {
                        Console.WriteLine("Action 4 is cancelled");
                        break;
                    }
                    if (cooperationMechanism4.IsPaused())
                    {
                        Console.WriteLine("Action 4 is paused");
                        Task.Delay(cooperationMechanism4.Paused).Wait();
                    }
                    Console.WriteLine("Action 4 is executing");
                    Thread.Sleep(100);

                }
            }

            coreTaskScheduler.QueueForScheduling(new List<PrioritizedLimitedTask>()
             {
                 new PrioritizedLimitedTask(action1, Priority.Lowest, 1800)
                 {
                     CooperationMechanism = cooperationMechanism1
                 },
                 new PrioritizedLimitedTask(action2, Priority.High, 3000)
                 {
                     CooperationMechanism = cooperationMechanism2
                 },
                  new PrioritizedLimitedTask(action3, Priority.Normal, 2000)
                  {
                      CooperationMechanism = cooperationMechanism3
                  }
             });
            Thread.Sleep(300);
            PrioritizedLimitedTask prioritizedLimitedTask = new PrioritizedLimitedTask(action4, Priority.Highest, 500)
            {
                CooperationMechanism = cooperationMechanism4
            };
            prioritizedLimitedTask.Start(coreTaskScheduler);
            Task task = new Task(() =>
            {
                Task.Delay(12000).Wait();
                Console.WriteLine(cooperationMechanism1.IsCancelled + "" + cooperationMechanism2.IsCancelled + cooperationMechanism3.IsCancelled + "");
            });
            task.Start();
            Task.WaitAll(task);
        }
    }
}
