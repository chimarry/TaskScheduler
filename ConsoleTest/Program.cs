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
            PreemptiveTaskScheduler coreTaskScheduler = new PreemptiveTaskScheduler(3);
            CooperationMechanism cooperationMechanism1 = new CooperationMechanism();
            cooperationMechanism1.CanBePaused = true;
            void action1()
            {
                for (int i = 0; i < 10; ++i)
                {
                    Console.WriteLine("Action 1 is executing step " + i);
                    Thread.Sleep(200);
                    if (cooperationMechanism1.IsCancelled)
                    {
                        Console.WriteLine("Action 1 is cancelled");
                        break;
                    }
                    if (cooperationMechanism1.IsPaused)
                    {
                        Console.WriteLine("Action 1 is paused at step " + i);
                        Task.Delay(cooperationMechanism1.PausedFor).Wait();
                    }
                }
            }

            CooperationMechanism cooperationMechanism2 = new CooperationMechanism();
            cooperationMechanism2.CanBePaused = true;
            void action2()
            {
                for (int i = 0; i < 15; ++i)
                {
                    Console.WriteLine("Action 2 is executing step " + i);
                    Thread.Sleep(200);
                    if (cooperationMechanism2.IsCancelled)
                    {
                        Console.WriteLine("Action 2 is cancelled");
                        break;
                    }
                    if (cooperationMechanism2.IsPaused)
                    {
                        Console.WriteLine("Action 2 is paused at step " + i);
                        Task.Delay(cooperationMechanism2.PausedFor).Wait();
                    }
                }
            }

            CooperationMechanism cooperationMechanism3 = new CooperationMechanism();
            cooperationMechanism3.CanBePaused = true;
            void action3()
            {
                for (int i = 0; i < 15; ++i)
                {
                    Console.WriteLine("Action 3 is executing step " + i);
                    Thread.Sleep(200);
                    if (cooperationMechanism3.IsCancelled)
                    {
                        Console.WriteLine("Action 3 is cancelled");
                        return;
                    }
                    while (cooperationMechanism3.IsPaused)
                    {
                        Console.WriteLine("Action 3 is paused at step " + i);
                        Task.Delay(cooperationMechanism3.PausedFor).Wait();
                    }
                }
            }


            CooperationMechanism cooperationMechanism4 = new CooperationMechanism();
            cooperationMechanism4.CanBePaused = true;
            void action4()
            {
                for (int i = 0; i < 5; ++i)
                {
                    Console.WriteLine("Action 4 is executing step " + i);
                    Thread.Sleep(200);
                    if (cooperationMechanism4.IsCancelled)
                    {
                        Console.WriteLine("Action 4 is cancelled");
                        break;
                    }
                    if (cooperationMechanism4.IsPaused)
                    {
                        Console.WriteLine("Action 4 is paused");
                        Task.Delay(cooperationMechanism4.PausedFor).Wait();
                    }
                }
            }

            CooperationMechanism cooperationMechanism5 = new CooperationMechanism();
            cooperationMechanism5.CanBePaused = true;
            void action5()
            {
                for (int i = 0; i < 10; ++i)
                {
                    Console.WriteLine("Action 5 is executing step " + i);
                    Thread.Sleep(200);
                    if (cooperationMechanism5.IsCancelled)
                    {
                        Console.WriteLine("Action 5 is cancelled");
                        break;
                    }
                    if (cooperationMechanism5.IsPaused)
                    {
                        Console.WriteLine("Action 5 is paused");
                        Task.Delay(cooperationMechanism4.PausedFor).Wait();
                    }
                }
            }
            // 1, 2, 5, 3 => 12 12 12 12 ... EXE: 1,2 pend: 5,3
            // 4,1,2,5,3 => 41 41 41 41 ... EXE: 1,4 pend: 5, 3, 2 (sort)=> 5, 2, 3
            // EXE: 4,5, EXE: 5,2 EXE: 2,3
            coreTaskScheduler.QueueForScheduling(new List<PrioritizedLimitedTask>()
                        {
                                  new PrioritizedLimitedTask(action1, Priority.High, 2000)
                                       {
                                        CooperationMechanism = cooperationMechanism1
                                        },
                                  new PrioritizedLimitedTask(action2, Priority.High, 3000)
                                       {
                                        CooperationMechanism = cooperationMechanism2
                                       },
                                   new PrioritizedLimitedTask(action3, Priority.Normal, 3000)
                                        {
                                        CooperationMechanism = cooperationMechanism3
                                        },
                                   new PrioritizedLimitedTask(action5,Priority.High,2000){
                                   CooperationMechanism = cooperationMechanism5
                                   }
                         });
            PrioritizedLimitedTask prioritizedLimitedTask = new PrioritizedLimitedTask(action4, Priority.Highest, 1000)
            {
                CooperationMechanism = cooperationMechanism4
            };
            prioritizedLimitedTask.Start(coreTaskScheduler);
            Task task = new Task(() =>
            {
                Task.Delay(10000).Wait();
                Console.WriteLine(cooperationMechanism1.IsCancelled + "" + cooperationMechanism2.IsCancelled + cooperationMechanism3.IsCancelled + "");
            });
            task.Start();
            Task.WaitAll(task);
        }
    }
}
