using Microsoft.VisualStudio.TestTools.UnitTesting;
using Scheduler;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Tests
{

    [TestClass]
    public class SharedResourceManager
    {
        [DataTestMethod]
        [DataRow(new int[] { 0, 10, 1, 5, 2, 7 }, new int[] { 0, 1, 2, 3, 4 }, new int[] { 7, 5, 3, 3, 2, 2, 9, 0, 2, 2, 2, 2, 4, 3, 3 }, new int[] { 0, 1, 0, 2, 0, 0, 3, 0, 2, 2, 1, 1, 0, 0, 2 }, new int[] { 0, 1, 0, 2 }, RequestApproval.Approved)]
        [DataRow(new int[] { 0, 1, 1, 3, 2, 2 }, new int[] { 0, 1, 2, 3, 4 }, new int[] { 7, 5, 3, 6, 2, 2, 9, 0, 2, 2, 2, 2, 4, 3, 3 }, new int[] { 0, 1, 0, 2, 0, 0, 3, 0, 2, 2, 1, 1, 0, 0, 2 }, new int[] { 0, 1, 0, 1 }, RequestApproval.Wait)]
        [DataRow(new int[] { 0, 1, 1, 3, 2, 2 }, new int[] { 0, 1, 2, 3, 4 }, new int[] { 7, 5, 3, 6, 2, 2, 9, 0, 2, 2, 2, 2, 4, 3, 3 }, new int[] { 0, 1, 0, 2, 0, 0, 3, 0, 2, 2, 1, 1, 0, 0, 2 }, new int[] { 0, 10, 12, 1 }, RequestApproval.Denied)]
        public void AllocateResource(int[] resources, int[] processes, int[] maximum, int[] allocated, int[] request, RequestApproval expectedApproval)
        {
            IBankerAlgorithm sharedResourceManager = new Scheduler.BankerAlgorithm();
            int resourceCount = resources.Length / 2;
            int processCount = processes.Length;

            // Register vailable resources
            for (int i = 0; i < resourceCount; i += 2)
                sharedResourceManager.RegisterResource(new SharedResource(resources[i]), resources[i + 1]);

            // Register processes
            int[,] maximumMatrix = ConvertToMatrix(maximum, processCount, resourceCount);

            for (int i = 0; i < processCount; ++i)
            {
                Dictionary<int, int> maximumResources = new Dictionary<int, int>();
                for (int j = 0; j < resourceCount; j += 2)
                    maximumResources.Add(resources[j], maximumMatrix[i, j]);
                sharedResourceManager.RegisterProcess(processes[i], maximumResources);
            }

            int[,] allocationMatrix = ConvertToMatrix(allocated, processCount, resourceCount);
            for (int i = 0; i < processCount; ++i)
            {
                Dictionary<int, int> allocationResources = new Dictionary<int, int>();
                for (int j = 0; j < resourceCount; j += 2)
                    allocationResources.Add(resources[j], allocationMatrix[i, j]);
                sharedResourceManager.ReinitializeAllocation(processes[i], allocationResources);
            }

            Dictionary<int, int> requestResources = new Dictionary<int, int>();
            for (int i = 1; i < processCount - 1; ++i)
                requestResources.Add(resources[(i - 1) * 2], request[i]);
            RequestApproval approval = sharedResourceManager.AllocateResources(request[0], requestResources);
            Assert.AreEqual(expectedApproval, approval);
        }

        private int[,] ConvertToMatrix(int[] flat, int m, int n)
        {
            if (flat.Length != m * n)
                throw new ArgumentException("Invalid length");

            int[,] ret = new int[m, n];
            Buffer.BlockCopy(flat, 0, ret, 0, flat.Length * sizeof(int));
            return ret;
        }
    }
}

class SharedResource : ISharedResource
{
    private int resourceIdentifier { get; }

    public SharedResource(int resourceId)
    {
        resourceIdentifier = resourceId;
    }

    public int GetResourceIdentifier() => resourceIdentifier;
}

class Process : Task
{
    public int ProcessIdentifier { get; private set; }
    public Process(Action action, int processIdentifier) : base(action)
    {
        ProcessIdentifier = processIdentifier;
    }
}