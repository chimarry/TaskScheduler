namespace Scheduler
{
    /// <summary>
    /// Implement this interface when you want to declare a system's shared resource.
    /// </summary>
    public interface ISharedResource
    {
        /// <summary>
        /// Returns identifier for a shared resource. Identifier is unique on a class level, that means
        /// if there are "n" instances of a shared resource, there will be one unique identifier for all of them.
        /// </summary>
        int GetResourceIdentifier();
    }
}
