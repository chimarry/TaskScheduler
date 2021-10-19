using System.Collections.Generic;

namespace Scheduler
{
    public interface IBankerAlgorithm
    {
        void RegisterResource(ISharedResource sharedResource, int instancesCount);

        void RegisterProcess(int processIdentifier, Dictionary<int, int> sharedResources);

        RequestApproval AllocateResources(int resourceIdentifier, Dictionary<int, int> sharedResources);

        void ReinitializeAllocation(int processIdentifier, Dictionary<int, int> allocatedResources);

        void FreeResources(int processIdentifier, Dictionary<int, int> sharedResources);
    }
}