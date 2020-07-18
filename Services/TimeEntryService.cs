using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractMechanics.TimeTracking.Models;
using AbstractMechanics.TimeTracking.Models.Dtos;
using Microsoft.Azure.Cosmos.Table;

namespace AbstractMechanics.TimeTracking.Services
{
    // ReSharper disable once ClassNeverInstantiated.Global
    public class TimeEntryService
    {
        private readonly Tables.TimeEntryTable _timeEntryTable;

        public TimeEntryService(Tables.TimeEntryTable timeEntryTable)
        {
            _timeEntryTable = timeEntryTable;
        }
        
        public async Task InsertEntry(string email, TimeEntryDto timeEntryDto)
        {
            var entity = new TimeEntry
            {
                PartitionKey = email,
                RowKey = timeEntryDto.DateTime.Ticks + "_" + Guid.NewGuid().ToString("n").Substring(0, 8),
                Amount = timeEntryDto.Amount,
                Unit = timeEntryDto.Unit,
                ProjectName = timeEntryDto.ProjectName
            };
            var operation = TableOperation.InsertOrReplace(entity);
            await _timeEntryTable.ExecuteAsync(operation);
        }
        
        public async Task<List<TimeEntryDto>> GetTimes(string email, DateTime startDate, DateTime endDate)
        {
            string pkFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, email);
            string rowFilterGt = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, startDate.Ticks.ToString());
            string rowFilterLt = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, endDate.Ticks.ToString());
            var query = new TableQuery<TimeEntry>().Where(TableQuery.CombineFilters(pkFilter, TableOperators.And,  
                TableQuery.CombineFilters(rowFilterGt, TableOperators.And, rowFilterLt)));
            var timeEntries = new List<TimeEntryDto>();
            TableContinuationToken token = null;
            do
            {
                var queryResults = await _timeEntryTable.ExecuteQuerySegmentedAsync(query, token);
                timeEntries.AddRange(queryResults.Select(r =>
                {
                    var dateTimeSection = r.RowKey.Split('_')[0];
                    long.TryParse(dateTimeSection, out long dateTimeLong);
                    return new TimeEntryDto()
                    {
                        DateTime = new DateTime(dateTimeLong),
                        ProjectName = r.ProjectName,
                        Amount = r.Amount,
                        Unit = r.Unit,
                    };
                }));
                token = queryResults.ContinuationToken;
            } while (token != null);
            return timeEntries;
        }
    }
}