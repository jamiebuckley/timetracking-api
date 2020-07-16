using System;
using System.Threading.Tasks;
using AbstractMechanics.TimeTracking.Models;
using AbstractMechanics.TimeTracking.Models.Dtos;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AbstractMechanics.TimeTracking.Functions.Projects
{
  public static class CreateProject
  {

    public static async Task<Project> InsertProject(CloudTable cloudTable, String partitionKey, CreateProjectDto body)
    {
        var entity = new Project();
        entity.PartitionKey = partitionKey;
        entity.RowKey = body.Name;
        var operation = TableOperation.InsertOrReplace(entity);
        await cloudTable.ExecuteAsync(operation);
        return entity;
    }

    [FunctionName("CreateProject")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "projects")] HttpRequest req,
        [Table("testTable")] CloudTable cloudTable,
        ILogger log)
    {

      if (!req.Headers.ContainsKey("auth")) return new BadRequestObjectResult("Missing auth header");
      try
      {
        var validPayload = await GoogleJsonWebSignature.ValidateAsync(req.Headers["auth"]);
        string data = await req.ReadAsStringAsync();
        var projectCreationRequest = JsonConvert.DeserializeObject<CreateProjectDto>(data);
        await InsertProject(cloudTable, validPayload.Email, projectCreationRequest);
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
