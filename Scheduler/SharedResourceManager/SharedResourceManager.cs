using System;
using System.Collections.Generic;
using System.Linq;

namespace Scheduler.SharedResourceManager
{
    /// <summary>
    /// Using a banker's algorithm, this class manages.
    /// </summary>
    public class SharedResourceManager : ISharedResourceManager
    {
        private static readonly object bankerAlgorithmLocker = new object();

        /// <summary>
        /// Collection that stores information about shared resources that are needed for 
        /// deadlock prevention. Key is an unique identifier of the resource, and value is 
        /// a number of instances of that resource.
        /// </summary>
        private Dictionary<int, int> availableSharedResources = new Dictionary<int, int>();

        /// <summary>
        /// Collection that stores information about processes.
        /// </summary>
        private readonly List<int> registeredProcesses = new List<int>();

        /// <summary>
        /// Represents matrix that defines the maximum demand of each process in a system.
        /// </summary>
        private readonly Dictionary<int, Dictionary<int, int>> maximumDemandes = new Dictionary<int, Dictionary<int, int>>();

        /// <summary>
        /// Represents matrix that defines the number of resources of each type currently allocated to each process.
        /// </summary>
        private Dictionary<int, Dictionary<int, int>> allocatedResources = new Dictionary<int, Dictionary<int, int>>();

        /// <summary>
        /// Represents matrix that indicates the remaining resource need of each process.
        /// </summary>
        private readonly Dictionary<int, Dictionary<int, int>> neededResources = new Dictionary<int, Dictionary<int, int>>();

        /// <summary>
        /// It is meant to be used by a process that wants to access a shared resources. Depending on the success of an allocation, 
        /// appropriate enum value <see cref="RequestApproval"/> will be returned. If the process demands more resource's instances 
        /// than what is it said it will use, the request will be denied.
        /// </summary>
        /// <param name="processIdentifier">Unique identifier for the process in the system</param>
        /// <param name="sharedResource">Information about the shared resource that process is requesting</param>
        /// <returns>Appropriate enum value <see cref="RequestApproval"/></returns>
        public RequestApproval AllocateResources(int processIdentifier, Dictionary<int, int> sharedResources)
        {
            lock (bankerAlgorithmLocker)
            {
                static bool greaterThan(int x, int y) => x > y;

                if (sharedResources.SatisfiesCondition(neededResources[processIdentifier], greaterThan))
                    return RequestApproval.Denied;

                else if (sharedResources.SatisfiesCondition(this.availableSharedResources, greaterThan))
                    return RequestApproval.Wait;

                // Save the previous state
                Dictionary<int, int> sharedResourcesOriginal = new Dictionary<int, int>(availableSharedResources);
                Dictionary<int, int> allocatedResourcesOriginal = new Dictionary<int, int>(allocatedResources[processIdentifier]);
                Dictionary<int, int> neededResourcesOriginal = new Dictionary<int, int>(neededResources[processIdentifier]);


                // Update resources
                availableSharedResources.ApplyOperation(sharedResources, (x, y) => x - y);
                allocatedResources[processIdentifier].ApplyOperation(sharedResources, (x, y) => x + y);
                neededResources[processIdentifier].ApplyOperation(sharedResources, (x, y) => x - y);

                bool noDeadlock = IsInSafeState();
                if (!noDeadlock)
                {
                    availableSharedResources = sharedResourcesOriginal;
                    allocatedResources[processIdentifier] = allocatedResourcesOriginal;
                    neededResources[processIdentifier] = neededResourcesOriginal;
                    return RequestApproval.Wait;
                }
                return RequestApproval.Approved;
            }
        }

        /// <summary>
        /// Process can free an allocated shared resources by calling this method. If process does not need 
        /// to access shared resources after calling this method, it will become unregistered.
        /// </summary>
        /// <param name="processIdentifier">Unique identifier for the process</param>
        /// <param name="sharedResources">Information about the allocated shared resources, like unique identifier for a
        /// resource and number of instances that process consumed</param>
        public void FreeResources(int processIdentifier, Dictionary<int, int> sharedResources)
        {
            lock (bankerAlgorithmLocker)
            {
                availableSharedResources.ApplyOperation(sharedResources, (x, y) => x + y);
                neededResources[processIdentifier].ApplyOperation(sharedResources, (x, y) => x - y);

                // If process has finished working with shared resources, remove any knowledge about it
                if (!neededResources[processIdentifier].AsParallel().Select(resource => resource.Value).Any(need => need != 0))
                {
                    registeredProcesses.Remove(registeredProcesses.First(process => process == processIdentifier));
                    maximumDemandes.Remove(processIdentifier);
                    neededResources.Remove(processIdentifier);
                    allocatedResources.Remove(processIdentifier);
                }
            }
        }

        /// <summary>
        /// This method changes allocation table for a specific process.
        /// </summary>
        /// <param name="processIdentifier">Unique identifier for the process</param>
        /// <param name="allocatedResources">Information about resources' allocation</param>
        public void ReinitializeAllocation(int processIdentifier, Dictionary<int, int> allocatedResources)
        {
            lock (bankerAlgorithmLocker)
            {
                this.allocatedResources[processIdentifier] = allocatedResources;
            }
        }

        /// <summary>
        /// Process needs to be registered to be able to access shared resources.
        /// </summary>
        /// <param name="processIdentifier">Unique identifier for the process</param>
        /// <param name="sharedResources">Information about shared resources that will be needed by the process 
        /// (maximum number of instances that the process may demand)</param>
        public void RegisterProcess(int processIdentifier, Dictionary<int, int> sharedResources)
        {
            lock (bankerAlgorithmLocker)
            {
                registeredProcesses.Add(processIdentifier);
                maximumDemandes.Add(processIdentifier, sharedResources);
                neededResources.Add(processIdentifier, sharedResources);

                int noAllocation = 0;
                Dictionary<int, int> allocatedResources = new Dictionary<int, int>();
                foreach (int resourceIdentifier in sharedResources.Keys)
                    allocatedResources.Add(resourceIdentifier, noAllocation);
                if (allocatedResources.ContainsKey(processIdentifier))
                    this.allocatedResources[processIdentifier] = allocatedResources;
                else
                    this.allocatedResources.Add(processIdentifier, allocatedResources);
            }
        }

        /// <summary>
        /// To be able to control access to a shared resource, that resource must be registered.
        /// </summary>
        /// <param name="sharedResource">Resource that needs to be registered (Only identifier is necessary)</param>
        /// <param name="instancesCount">Number of available instances of that resource</param>
        public void RegisterResource(ISharedResource sharedResource, int instancesCount)
        {
            lock (bankerAlgorithmLocker)
            {
                if (!availableSharedResources.ContainsKey(sharedResource.GetResourceIdentifier()))
                    availableSharedResources.Add(sharedResource.GetResourceIdentifier(), instancesCount);
            }
        }

        /// <summary>
        /// Using Banker's algorithm, this method checks whether the system will be in the safe state after 
        /// process accesses shared resources.
        /// </summary>
        /// <returns>True if system will remain in the safe state, and false if not</returns>
        private bool IsInSafeState()
        {
            Dictionary<int, int> work = new Dictionary<int, int>(availableSharedResources);
            Dictionary<int, bool> finished = new Dictionary<int, bool>();
            registeredProcesses.ForEach(processIdentifier => finished.Add(processIdentifier, false));

            for (int processId = 0;
                    processId < registeredProcesses.Count
                    && !finished[processId]
                    && neededResources[processId].SatisfiesCondition(work, (x, y) => x <= y);
                 ++processId)
            {
                work.ApplyOperation(availableSharedResources, (x, y) => x + y);
                finished[processId] = true;
            }
            return !finished.Any(x => !x.Value);
        }
    }
}

public static class DictionaryUtil
{
    public static bool SatisfiesCondition(this Dictionary<int, int> original, Dictionary<int, int> request, Func<int, int, bool> conditionFunc)
    {
        foreach (int key in request.Keys)
            if (!conditionFunc.Invoke(original[key], request[key]))
                return false;
        return true;
    }

    public static void ApplyOperation(this Dictionary<int, int> original, Dictionary<int, int> modifier, Func<int, int, int> operation)
    {
        Dictionary<int, int> result = new Dictionary<int, int>(original);
        foreach (int resourceKey in original.Keys)
            result[resourceKey] = operation.Invoke(original[resourceKey], modifier[resourceKey]);
    }
}