namespace Scheduler
{
    public interface IRealTimeScheduler
    {
        void QueueForScheduling(PrioritizedLimitedTask prioritizedLimitedAction);
    }
}
