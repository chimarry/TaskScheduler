using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scheduler;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Tests
{
    [TestClass]
    public class BasicScheduler
    {
        [TestMethod]
        public void RunNonPreemptive()
        {
            PreemptiveTaskScheduler coreTaskScheduler = new PreemptiveTaskScheduler(2);
            CooperationMechanism cooperationMechanism1 = new CooperationMechanism();
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
                }
            }

            CooperationMechanism cooperationMechanism2 = new CooperationMechanism();
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
                }
            }

            CooperationMechanism cooperationMechanism3 = new CooperationMechanism();
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
                }
            }


            CooperationMechanism cooperationMechanism4 = new CooperationMechanism();
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
                }
            }
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
                                      }
                            });
            PrioritizedLimitedTask prioritizedLimitedTask = new PrioritizedLimitedTask(action4, Priority.Highest, 1000)
            {
                CooperationMechanism = cooperationMechanism4
            };
            // Real-time scheduling
            prioritizedLimitedTask.Start(coreTaskScheduler);
            Thread.Sleep(10000);
            Assert.IsTrue(cooperationMechanism1.IsCancelled);
            Assert.IsTrue(cooperationMechanism2.IsCancelled);
            Assert.IsTrue(cooperationMechanism3.IsCancelled);
            Assert.IsTrue(cooperationMechanism4.IsCancelled);
        }

        [TestMethod]
        public void RunPreemtive()
        {
            PreemptiveTaskScheduler coreTaskScheduler = new PreemptiveTaskScheduler(2);
            CooperationMechanism cooperationMechanism1 = new CooperationMechanism();
            cooperationMechanism1.CanBePaused = true;
            void action1()
            {
                for (int i = 0; i < 20; ++i)
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
            coreTaskScheduler.QueueForScheduling(new List<PrioritizedLimitedTask>()
                           {
                                     new PrioritizedLimitedTask(action1, Priority.High, 4000)
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
                Task.Delay(2000).Wait();
                Assert.IsTrue(cooperationMechanism2.IsPaused || cooperationMechanism1.IsPaused, "Not paused");
                Assert.IsTrue(cooperationMechanism4.IsCancelled, "Not finished");
            });
            task.Start();
            Task.WaitAll(task);
        }
    }
}
