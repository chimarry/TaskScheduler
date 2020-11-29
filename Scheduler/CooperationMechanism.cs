namespace Scheduler
{
    /// <summary>
    /// Mechanism for cooperative task abandoning and context switching.
    /// </summary>
    public class CooperationMechanism
    {
        public bool IsAbandoned { get; set; }

        public void Abandone() => IsAbandoned = true;
    }
}
