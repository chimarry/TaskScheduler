using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Scheduler
{
    public class CoreTaskScheduler : TaskScheduler, IRealTimeScheduler
    { 
        protected override IEnumerable<Task> GetScheduledTasks()
        {
            throw new NotImplementedException();
        }

        protected override void QueueTask(Task task)
        {
            throw new NotImplementedException();
        }

        protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
        {
            throw new NotImplementedException();
        }
    }
}
