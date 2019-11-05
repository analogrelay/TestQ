using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.TeamFoundation.Build.WebApi;
using Microsoft.TeamFoundation.Test.WebApi;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.OAuth;
using Microsoft.VisualStudio.Services.TestResults.WebApi;
using Microsoft.VisualStudio.Services.WebApi;

namespace TestQ.Worker
{
    public class TestResultSyncJob : BackgroundService
    {
        private static readonly TimeSpan Interval = TimeSpan.FromSeconds(1);

        private readonly ILogger<TestResultSyncJob> _logger;
        private readonly VssConnection _vss;
        private readonly IOptions<TestResultSyncOptions> _options;

        public TestResultSyncJob(ILogger<TestResultSyncJob> logger, VssConnection vss, IOptions<TestResultSyncOptions> options)
        {
            _logger = logger;
            _vss = vss;
            _options = options;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var testClient = _vss.GetClient<TestResultsHttpClient>();
            var buildsClient = _vss.GetClient<BuildHttpClient>();

            // Resolve the builds
            var taskContexts = new List<TaskContext>();
            foreach(var task in _options.Value.Tasks)
            {
                var definitions = await buildsClient.GetDefinitionsAsync2(
                    project: task.Project,
                    name: task.Pipeline);
                if(definitions.Count > 1)
                {
                    _logger.LogError("Found multiple results for pipeline '{Name}' in project {Project}'. Skipping.", task.Pipeline, task.Project);
                    // Skip
                }
                else if(definitions.Count == 0)
                {
                    _logger.LogError("Found no results for pipeline '{Name}' in project {Project}'. Skipping.", task.Pipeline, task.Project);
                    // Skip
                }
                else
                {
                    taskContexts.Add(new TaskContext(task, definitions[0]));
                }
            }

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Test result sync job running at: {Time}", DateTimeOffset.Now);

                foreach(var taskContext in taskContexts)
                {
                }

                _logger.LogDebug("Sleeping for {Interval}...", Interval);
                await Task.Delay(Interval, stoppingToken);
            }
        }

        private class TaskContext
        {
            public TaskContext(TestResultSyncTaskOptions task, BuildDefinitionReference buildDefinitionReference)
            {
                Task = task;
                BuildDefinitionReference = buildDefinitionReference;
            }

            public TestResultSyncTaskOptions Task { get; }
            public BuildDefinitionReference BuildDefinitionReference { get; }
        }
    }
}
