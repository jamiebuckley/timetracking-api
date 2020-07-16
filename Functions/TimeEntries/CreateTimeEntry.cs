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

namespace AbstractMechanics.TimeTracking.Functions.TimeEntries
{
  public static class CreateTimeEntry
  {

    public static async Task InsertEntry(CloudTable cloudTable, string email, TimeEntryDto timeEntryDTO)
    {
      var entity = new TimeEntry();
      entity.PartitionKey = email;
      entity.RowKey = timeEntryDTO.DateTime.Ticks.ToString();
      entity.Amount = timeEntryDTO.Amount;
      entity.Unit = timeEntryDTO.Unit;
      entity.ProjectName = timeEntryDTO.ProjectName;
      var operation = TableOperation.InsertOrReplace(entity);
      await cloudTable.ExecuteAsync(operation);
    }

    [FunctionName("CreateTimeEntry")]
    public static async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "timeEntries")] HttpRequest req,
        [Table("timeentries")] CloudTable cloudTable,
        ILogger log)
    {
      if (!req.Headers.ContainsKey("auth")) return new BadRequestObjectResult("Missing auth header");
      try
      {
        var validPayload = await GoogleJsonWebSignature.ValidateAsync(req.Headers["auth"]);
        string data = await req.ReadAsStringAsync();
        var timeEntryCreationRequest = JsonConvert.DeserializeObject<TimeEntryDto>(data);
        try {
        await InsertEntry(cloudTable, validPayload.Email, timeEntryCreationRequest);
        } catch (Exception ex) {
            log.LogError(ex.ToString());
        }
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
