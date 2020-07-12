using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Microsoft.Azure.Cosmos.Table;
using Google.Apis.Auth;

namespace AbstractMechanics.TimeTracking.Function
{
  public static class DeleteProject
  {
    public class DeleteProjectDTO
    {
      public string Name { get; set; }
    }

    public static async Task<TableResult> RemoveProject(CloudTable cloudTable, String partitionKey, DeleteProjectDTO body)
    {
        var operation = TableOperation.Delete(new TableEntity() { PartitionKey = partitionKey, RowKey = body.Name, ETag = "*" });
        var result = await cloudTable.ExecuteAsync(operation);
        return result;
    }

    [FunctionName("DeleteProject")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = null)] HttpRequest req,
        [Table("testTable")] CloudTable cloudTable,
        ILogger log)
    {
      if (!req.Headers.ContainsKey("auth")) return new BadRequestObjectResult("Missing auth header");
      try
      {
        var validPayload = await GoogleJsonWebSignature.ValidateAsync(req.Headers["auth"]);
        string data = await req.ReadAsStringAsync();
        var projectDeletionRequest = JsonConvert.DeserializeObject<DeleteProjectDTO>(data);
        var deletionResult = RemoveProject(cloudTable, validPayload.Email, projectDeletionRequest);
        return new OkResult();
      }
      catch (InvalidJwtException ex)
      {
        log.LogError(ex.ToString());
        return new UnauthorizedResult();
      }
    }
  }
}
