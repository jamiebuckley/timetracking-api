using System;
using System.IO;
using System.Reflection;
using AbstractMechanics.TimeTracking.Config;
using AbstractMechanics.TimeTracking.Services;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.Functions.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

[assembly: FunctionsStartup(typeof( AbstractMechanics.TimeTracking.Startup))]
namespace AbstractMechanics.TimeTracking
{
    public class Startup : FunctionsStartup
    {
        public override void Configure(IFunctionsHostBuilder builder)
        {
            var config = new ConfigurationBuilder()
                .SetBasePath(Environment.GetEnvironmentVariable("AzureWebJobsScriptRoot") ?? Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location))
                .AddJsonFile("local.settings.json", optional: true)
                .AddEnvironmentVariables()
                .Build();

            var cloudTableOptions = new CloudTableConfig();
            config.Bind(nameof(CloudTableConfig), cloudTableOptions);
            if (cloudTableOptions.ConnectionString == null)
                throw new InvalidOperationException("No cloudtable connection string provided in config");

            var cloudStorageAccount = CloudStorageAccount.Parse(cloudTableOptions.ConnectionString);
            var cloudTableClient = cloudStorageAccount.CreateCloudTableClient();
            builder.Services.AddSingleton(new Tables.ProjectTable(cloudTableClient.GetTableReference("projects")));
            builder.Services.AddSingleton(new Tables.TimeEntryTable(cloudTableClient.GetTableReference("timeEntries")));
            
            builder.Services.AddSingleton<ProjectService>();
            builder.Services.AddSingleton<TimeEntryService>();
        }
    }
}