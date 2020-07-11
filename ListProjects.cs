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

namespace AbstractMechanics.TimeTracking.Function
{
  public static class ListProjects
  {

    public class ProjectResponseItem
    {
        public string Name { get; set; }
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
        string pkFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, validPayload.Email);
        var query = new TableQuery().Where(pkFilter);
        var queryResults = await cloudTable.ExecuteQuerySegmentedAsync(query, null);
        var results = queryResults.Select(r => new ProjectResponseItem() { Name = r.RowKey }).ToList();
        return new OkObjectResult(results);
      }
      catch (InvalidJwtException ex)
      {
        log.LogError(ex.ToString());
        return new UnauthorizedResult();
      }
    }
  }
}
