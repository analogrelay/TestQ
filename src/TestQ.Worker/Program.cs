using System;
using System.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Microsoft.VisualStudio.Services.Common;
using Microsoft.VisualStudio.Services.WebApi;

namespace TestQ.Worker
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureServices((hostContext, services) =>
                {
                    services.AddSingleton((s) =>
                    {
                        var options = s.GetRequiredService<IOptions<AzureDevOpsOptions>>();
                        var creds = new VssBasicCredential(string.Empty, options.Value.AccessToken);
                        return new VssConnection(new Uri(options.Value.OrganizationUrl), creds);
                    });

                    services.Configure<TestResultSyncOptions>(hostContext.Configuration.GetSection("Jobs:TestResultSync"));
                    services.Configure<AzureDevOpsOptions>(hostContext.Configuration.GetSection("AzDO"));
                    services.AddHostedService<TestResultSyncJob>();
                });
    }
}
