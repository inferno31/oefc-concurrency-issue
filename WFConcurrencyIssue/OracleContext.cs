using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.Extensions.Logging;
using WorkflowCore.Persistence.EntityFramework.Models;
using WorkflowCore.Persistence.EntityFramework.Services;

namespace WorkflowCore.Persistence.Oracle
{
    public class OracleContext : WorkflowDbContext
    {
        private readonly string _connectionString;

        public OracleContext(DbContextOptions<OracleContext> options)
            : base(options)
        { }

        protected override void ConfigureSubscriptionStorage(EntityTypeBuilder<PersistedSubscription> builder)
        {
            builder.ToTable("Subscription", "WFC");
            builder.Property(x => x.PersistenceId).UseOracleIdentityColumn();
        }

        protected override void ConfigureWorkflowStorage(EntityTypeBuilder<PersistedWorkflow> builder)
        {
            builder.ToTable("Workflow", "WFC");
            builder.Property(x => x.PersistenceId).UseOracleIdentityColumn();
        }

        protected override void ConfigureExecutionPointerStorage(EntityTypeBuilder<PersistedExecutionPointer> builder)
        {
            builder.ToTable("ExecutionPointer", "WFC");
            builder.Property(x => x.PersistenceId).UseOracleIdentityColumn();
        }

        protected override void ConfigureExecutionErrorStorage(EntityTypeBuilder<PersistedExecutionError> builder)
        {
            builder.ToTable("ExecutionError", "WFC");
            builder.Property(x => x.PersistenceId).UseOracleIdentityColumn();
        }

        protected override void ConfigureExetensionAttributeStorage(EntityTypeBuilder<PersistedExtensionAttribute> builder)
        {
            builder.ToTable("ExtensionAttribute", "WFC");
            builder.Property(x => x.PersistenceId).UseOracleIdentityColumn();
        }

        protected override void ConfigureEventStorage(EntityTypeBuilder<PersistedEvent> builder)
        {
            builder.ToTable("Event", "WFC");
            builder.Property(x => x.PersistenceId).UseOracleIdentityColumn();
        }
    }
}
