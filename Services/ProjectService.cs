using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractMechanics.TimeTracking.Models;
using AbstractMechanics.TimeTracking.Models.Dtos;
using Microsoft.Azure.Cosmos.Table;

namespace AbstractMechanics.TimeTracking.Services
{
    public class ProjectService
    {
        private readonly Tables.ProjectTable _projectTable;

        public ProjectService(Tables.ProjectTable projectTable)
        {
            _projectTable = projectTable;
        }
        
        public async Task<List<ProjectDto>> GetProjects(string partitionKey) {
            string pkFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
            var query = new TableQuery<Project>().Where(pkFilter);
            var projects = new List<ProjectDto>();
            TableContinuationToken token = null;
            do {
                var queryResults = await _projectTable.ExecuteQuerySegmentedAsync(query, token);
                projects.AddRange(queryResults.Select(r => new ProjectDto() { Name = r.RowKey, Color = r.Color }));
                token = queryResults.ContinuationToken;
            } while (token != null);
            return projects;
        }
        
        public async Task<Project> InsertProject(String partitionKey, CreateProjectDto body)
        {
            var entity = new Project();
            entity.PartitionKey = partitionKey;
            entity.RowKey = body.Name;
            entity.Color = body.Color;
            var operation = TableOperation.InsertOrReplace(entity);
            await _projectTable.ExecuteAsync(operation);
            return entity;
        }
        
        public async Task<TableResult> RemoveProject(String partitionKey, DeleteProjectDto body)
        {
            var operation = TableOperation.Delete(new TableEntity() { PartitionKey = partitionKey, RowKey = body.Name, ETag = "*" });
            return await _projectTable.ExecuteAsync(operation);
        }
    }
}