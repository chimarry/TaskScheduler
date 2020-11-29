using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;

namespace Scheduler
{
    public class PriorityComparer : IComparer<Priority>
    {
        /// <summary>
        /// Compares two priorities <see cref="Priority"/> using default enum comparator.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public int Compare([AllowNull] Priority x, [AllowNull] Priority y)
        {
            return x.CompareTo(y);
        }
    }
}
