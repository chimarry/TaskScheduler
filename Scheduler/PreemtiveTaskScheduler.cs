using System.Threading.Tasks;

namespace Scheduler
{
    public class PreemtiveTaskScheduler : CoreTaskScheduler
    {
        public PreemtiveTaskScheduler(int maxLevelOfParallelism) : base(maxLevelOfParallelism)
        {
        }

        public override void RunScheduling()
        {
            throw new System.NotImplementedException();
        }

        protected override void QueueTask(Task task)
        {
            throw new System.NotImplementedException();

        }
    }
}
