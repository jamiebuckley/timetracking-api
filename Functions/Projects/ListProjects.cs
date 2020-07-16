using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AbstractMechanics.TimeTracking.Functions.Projects
{
  public static class ListProjects
  {

    public class ProjectResponseItem
    {
        public string Name { get; set; }
    }

    private static async Task<List<ProjectResponseItem>> GetProjects(CloudTable cloudTable, string partitionKey) {
        string pkFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, partitionKey);
        var query = new TableQuery().Where(pkFilter);
        var projects = new List<ProjectResponseItem>();
        TableContinuationToken token = null;
        do {
          var queryResults = await cloudTable.ExecuteQuerySegmentedAsync(query, token);
          projects.AddRange(queryResults.Select(r => new ProjectResponseItem() { Name = r.RowKey }));
          token = queryResults.ContinuationToken;
        } while (token != null);
        return projects;
    }


    [FunctionName("ListProjects")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        [Table("testTable")] CloudTable cloudTable,
        ILogger log)
    {
      if (!req.Headers.ContainsKey("auth")) return new BadRequestObjectResult("Missing auth header");
      try
      {
        var validPayload = await GoogleJsonWebSignature.ValidateAsync(req.Headers["auth"]);
        var projects = await GetProjects(cloudTable, validPayload.Email);
        return new OkObjectResult(projects);
      }
      catch (InvalidJwtException ex)
      {
        log.LogError(ex.ToString());
        return new UnauthorizedResult();
      }
    }
  }
}
