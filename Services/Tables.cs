using System.Threading.Tasks;
using Microsoft.Azure.Cosmos.Table;

namespace AbstractMechanics.TimeTracking.Services
{
    public static class Tables
    {
        public abstract class TableBase
        {
            private CloudTable CloudTable { get; }

            protected TableBase(CloudTable cloudTable)
            {
                CloudTable = cloudTable;
            }
            
            public async Task<TableQuerySegment<DynamicTableEntity>> ExecuteQuerySegmentedAsync(TableQuery query, TableContinuationToken token)
            {
                return await CloudTable.ExecuteQuerySegmentedAsync(query, token);
            }
            
            public async Task<TableQuerySegment<T>> ExecuteQuerySegmentedAsync<T>(TableQuery<T> query, TableContinuationToken token) where T : ITableEntity, new()
            {
                return await CloudTable.ExecuteQuerySegmentedAsync(query, token);
            }
            
            public async Task ExecuteAsync(TableOperation operation)
            {
                await CloudTable.ExecuteAsync(operation);
            }
        }
        
        public class ProjectTable : TableBase
        {
            public ProjectTable(CloudTable cloudTable) : base(cloudTable) { }
        }
        
        public class TimeEntryTable : TableBase
        {
            public TimeEntryTable(CloudTable cloudTable) : base(cloudTable) { }
        }
    }
}