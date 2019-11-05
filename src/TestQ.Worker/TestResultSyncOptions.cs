using System.Collections;
using System.Collections.Generic;

namespace TestQ.Worker
{
    public class TestResultSyncOptions
    {
        public IList<TestResultSyncTaskOptions> Tasks { get; set }
    }

    public class TestResultSyncTaskOptions
    {
        public string Project { get; set; }
        public string Pipeline { get; set; }
    }
}