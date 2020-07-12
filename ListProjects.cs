using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Google.Apis.Auth;
using System.Linq;
using Microsoft.Azure.Cosmos.Table;
using System.Collections.Generic;

namespace AbstractMechanics.TimeTracking.Function
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
      log.Log(LogLevel.Error, "Processing request");
      if (!req.Headers.ContainsKey("auth"))
      {
        log.Log(LogLevel.Error, "No auth header");
        return new UnauthorizedResult();
      }
      var authorisation = req.Headers["auth"];

      try
      {
        var validPayload = await GoogleJsonWebSignature.ValidateAsync(authorisation);
        return new OkObjectResult(GetProjects(cloudTable, validPayload.Email));
      }
      catch (InvalidJwtException ex)
      {
        log.LogError(ex.ToString());
        return new UnauthorizedResult();
      }
    }
  }
}
