using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Linq;
using System.Threading.Tasks;
using WorkflowCore.Persistence.EntityFramework.Models;
using WorkflowCore.Persistence.Oracle;

namespace WFConcurrencyIssue
{
    class Program
    {
        static string connectionString = "Data Source=(DESCRIPTION=(ADDRESS_LIST=(ADDRESS=(PROTOCOL=TCP)(HOST=192.168.56.102)(PORT=32118)))" +
            "(CONNECT_DATA=(SERVER=DEDICATED)(SERVICE_NAME=xe)));User Id=WFC;Password=Oracle18;";

        static async Task Main(string[] args)
        {
            var serviceProvider = BuildServiceProvider();

            long wfId;

            // setup
            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<OracleContext>();

                context.Database.EnsureCreated();

                var wf = new PersistedWorkflow()
                {
                    CreateTime = DateTime.Now,
                    Data = "{\"$type\":\"WorkflowCore.Tests.Oracle.Scenarios.SimpleEventScenario + MyDataClass, WorkflowCore.Tests.Oracle\",\"StrValue1\":\"1b98c27f-f33c-48b3-bffa-a9f758f1e74c\",\"StrValue2\":\"0be48870-6741-467a-a3e1-f7becfc6e681\"}",
                    InstanceId = Guid.NewGuid(),
                    NextExecution = 0,
                    Status = 0,
                    Version = 1,
                    WorkflowDefinitionId = "EventWorkflow"
                };

                var predecessor = new PersistedExecutionPointer()
                {
                    Id = Guid.NewGuid().ToString(),
                    StartTime = DateTime.Now,
                    EndTime = DateTime.Now,
                    StepId = 0,
                    Active = false,
                    Status = PointerStatus.Complete
                };
                wf.ExecutionPointers.Add(predecessor);

                wf.ExecutionPointers.Add(new PersistedExecutionPointer()
                {
                    Id = Guid.NewGuid().ToString(),
                    Active = true,
                    StartTime = DateTime.Now,
                    EventKey = "1b98c27f-f33c-48b3-bffa-a9f758f1e74c",
                    EventName = "MyEvent",
                    EventPublished = true,
                    PredecessorId = predecessor.Id,
                    StepId = 1,
                    Status = PointerStatus.WaitingForEvent
                });

                context.Set<PersistedWorkflow>().Add(wf);
                await context.SaveChangesAsync();

                wfId = wf.PersistenceId;
            }

            using (var scope = serviceProvider.CreateScope())
            {
                var context = scope.ServiceProvider.GetRequiredService<OracleContext>();

                var wf = await context.Set<PersistedWorkflow>()
                    .Include(x => x.ExecutionPointers)
                    .FirstAsync(w => w.PersistenceId == wfId);

                wf.Data = "{\"$type\":\"WorkflowCore.Tests.Oracle.Scenarios.SimpleEventScenario+MyDataClass, WorkflowCore.Tests.Oracle\",\"StrValue1\":\"1b98c27f-f33c-48b3-bffa-a9f758f1e74c\",\"StrValue2\":\"0be48870-6741-467a-a3e1-f7becfc6e681\"}";
                
                var step0 = wf.ExecutionPointers.Single(e => e.StepId == 0);
                step0.Children = string.Empty;
                step0.Scope = string.Empty;

                var step1 = wf.ExecutionPointers.Single(e => e.StepId == 1);
                step1.Active = false;
                step1.Children = string.Empty;
                step1.Scope = string.Empty;
                step1.EndTime = DateTime.Now;

                wf.ExecutionPointers.Add(new PersistedExecutionPointer()
                {
                    Active = true,
                    Children = string.Empty,
                    ContextItem = "null",
                    EndTime = null,
                    EventData = "null",
                    EventKey = string.Empty,
                    EventName = string.Empty,
                    EventPublished = false,
                    Id = Guid.NewGuid().ToString(),
                    Outcome = "null",
                    PersistenceData = "null",
                    PredecessorId = step1.Id,
                    RetryCount = 0,
                    Scope = string.Empty,
                    SleepUntil = null,
                    StartTime = DateTime.Now,
                    Status = PointerStatus.Pending,
                    StepId = 2,
                    StepName = null,
                });
                
                await context.SaveChangesAsync();
            }
        }

        private static IServiceProvider BuildServiceProvider()
        {
            var services = new ServiceCollection();
            services.AddLogging(l =>
            {
                l.AddDebug();
                l.AddConsole();
            });

            services.AddDbContext<OracleContext>(o =>
            {
                o.UseOracle(connectionString);
            });

            return services.BuildServiceProvider();
        }
    }
}
