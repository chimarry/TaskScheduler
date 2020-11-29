using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scheduler;
using System;
using System.Collections.Generic;
using System.Text;

namespace Tests
{
    [TestClass]
    public class CoreTaskScheduler
    {
        [TestMethod]
        public void Schedule()
        {
            Scheduler.CoreTaskScheduler coreTaskScheduler = new Scheduler.CoreTaskScheduler(2);
            CooperationMechanism cooperationMechanism1 = new CooperationMechanism();
            Action action1 = () =>
            {
                while (true)
                {
                    Console.WriteLine("Action 1 is executing");
                    if (cooperationMechanism1.IsCancelled)
                        break;
                }
            };
            CooperationMechanism cooperationMechanism2 = new CooperationMechanism();
            Action action2 = () =>
            {
                while (true)
                {
                    Console.WriteLine("Action 2 is executing");
                    if (cooperationMechanism2.IsCancelled)
                        break;
                }
            };

            CooperationMechanism cooperationMechanism3 = new CooperationMechanism();
            Action action3 = () =>
            {
                while (true)
                {
                    Console.WriteLine("Action 3 is executing");
                    if (cooperationMechanism3.IsCancelled)
                        break;
                }
            };

            coreTaskScheduler.QueueForScheduling(new List<PrioritizedLimitedTask>()
            {
                new PrioritizedLimitedTask(action1, Priority.Lowest, 2000)
                {
                    CooperationMechanism = cooperationMechanism1
                },
                new PrioritizedLimitedTask(action2, Priority.High, 1000)
                {
                    CooperationMechanism = cooperationMechanism2
                },
                 new PrioritizedLimitedTask(action3, Priority.Normal, 1300)
                 {
                     CooperationMechanism = cooperationMechanism3
                 }
            });
        }
    }
}
