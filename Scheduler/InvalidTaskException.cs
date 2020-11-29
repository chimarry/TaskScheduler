using System;

namespace Scheduler
{
    public class InvalidTaskException : Exception
    {
        public InvalidTaskException() : base("Invalid type of a task") { }
    }
}
