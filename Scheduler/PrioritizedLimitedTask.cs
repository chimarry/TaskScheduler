using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace Scheduler
{
    /// <summary>
    /// Encapsulate an action passed to a task scheduler, with each action having its priority and max duration.
    /// </summary>
    public class PrioritizedLimitedTask : Task, IComparable<PrioritizedLimitedTask>
    {
        private const int miminumIdentifierValue = 28;
        private const int maximumIdentifierValue = 1997;

        public CooperationMechanism CooperationMechanism { get; set; }

        /// <summary>
        /// Unique identifier for an action
        /// </summary>
        public int PrioritizedLimitedTaskIdentifier { get; set; }

        /// <summary>
        /// An action to be executed
        /// </summary>
        public Action Action { get; set; }

        /// <summary>
        /// A priority of the action
        /// <see cref="Priority"/>
        /// </summary>
        public Priority Priority { get; set; }

        /// <summary>
        /// Maximum time for an execution of the action (given in miliseconds)
        /// </summary>
        public int DurationInMiliseconds { get; set; }

        public PrioritizedLimitedTask(Action action, Priority priority, int durationInMiliseconds) : base(action)
        {
            Priority = priority;
            DurationInMiliseconds = durationInMiliseconds;
            PrioritizedLimitedTaskIdentifier = new Random().Next(miminumIdentifierValue, maximumIdentifierValue);
            Action = action;
        }

        public int CompareTo(PrioritizedLimitedTask other)
        {
            return Priority.CompareTo(other.Priority);
        }
    }

    /// <summary>
    /// Represents possible priorities of an action.
    /// </summary>
    public enum Priority { Lowest, Low, Normal, High, Highest }
}
