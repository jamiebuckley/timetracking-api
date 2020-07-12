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
  public static class CreateTimeEntry
  {

    public static async Task InsertEntry(CloudTable cloudTable, string email, TimeEntryDTO timeEntryDTO)
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
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
        [Table("timeentries")] CloudTable cloudTable,
        ILogger log)
    {
      if (!req.Headers.ContainsKey("auth")) return new BadRequestObjectResult("Missing auth header");
      try
      {
        var validPayload = await GoogleJsonWebSignature.ValidateAsync(req.Headers["auth"]);
        string data = await req.ReadAsStringAsync();
        var timeEntryCreationRequest = JsonConvert.DeserializeObject<TimeEntryDTO>(data);
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
