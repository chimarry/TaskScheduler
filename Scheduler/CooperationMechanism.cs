namespace Scheduler
{
    /// <summary>
    /// Mechanism for cooperative task abandoning and context switching.
    /// </summary>
    public class CooperationMechanism
    {
        public bool IsCancelled { get; set; }

        public int Paused { get; set; }

        public void Cancel() => IsCancelled = true;

        public bool IsPaused() => Paused > 0;

        public void Resume()
        {
            Paused = 0;
        }
    }
}
