using System;

namespace Scheduler
{
    /// <summary>
    /// Mechanism for cooperative task abandoning and context switching.
    /// </summary>
    public class CooperationMechanism
    {
        public bool IsCancelled { get; set; }

        public int PausedFor { get; private set; } = 0;

        public bool IsPaused { get; private set; }

        public bool IsResumed { get; private set; }

        public void Cancel() => IsCancelled = true;

        public void Pause(int time)
        {
            PausedFor += time;
            IsPaused = true;
        }

        public void Resume()
        {
            IsPaused = false;
            IsResumed = true;
        }
    }
}
