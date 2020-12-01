namespace Scheduler
{
    /// <summary>
    /// Mechanism for cooperative task cancelling and context switching.
    /// </summary>
    public class CooperationMechanism
    {
        /// <summary>
        /// Indicates whether the task has been cooperatively cancelled.
        /// </summary>
        public bool IsCancelled { get; set; }

        /// <summary>
        /// Inidicates whether the task can be paused (used for context switching).
        /// </summary>
        public bool CanBePaused { get; set; }

        /// <summary>
        /// Indicates a total time that the task has been paused for.
        /// </summary>
        public int PausedFor { get; private set; } = 0;

        /// <summary>
        /// Indicates whether the task is paused.
        /// </summary>
        public bool IsPaused { get; private set; }

        /// <summary>
        /// Indicates whether the task has been resumed.
        /// </summary>
        public bool IsResumed { get; private set; }

        /// <summary>
        /// Task should be cancelled cooperatively.
        /// </summary>
        public void Cancel() => IsCancelled = true;

        /// <summary>
        /// Task should be paused cooperatively.
        /// </summary>
        /// <param name="time"></param>
        public void Pause(int time)
        {
            PausedFor += time;
            IsPaused = true;
        }

        /// <summary>
        /// Resume task.
        /// </summary>
        public void Resume()
        {
            IsPaused = false;
            IsResumed = true;
        }
    }
}
