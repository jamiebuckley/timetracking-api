using System;
using System.Collections.Generic;
using System.Linq;
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
  public static class GetTimeEntries
  {
    public class TimeQueryDto
    {
      public DateTime FromTime { get; set; }
      public DateTime ToTime { get; set; }
    }

    public static async Task<List<TimeEntryDto>> GetTimes(CloudTable cloudTable, string email, TimeQueryDto timeQueryDTO)
    {
      string pkFilter = TableQuery.GenerateFilterCondition("PartitionKey", QueryComparisons.Equal, email);
      string rowFilterGt = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.GreaterThanOrEqual, timeQueryDTO.FromTime.Ticks.ToString());
      string rowFilterLt = TableQuery.GenerateFilterCondition("RowKey", QueryComparisons.LessThanOrEqual, timeQueryDTO.ToTime.Ticks.ToString());
      var query = new TableQuery<TimeEntry>().Where(TableQuery.CombineFilters(pkFilter, TableOperators.And,  
      TableQuery.CombineFilters(rowFilterGt, TableOperators.And, rowFilterLt)));
      var timeEntries = new List<TimeEntryDto>();
      TableContinuationToken token = null;
      do
      {
        var queryResults = await cloudTable.ExecuteQuerySegmentedAsync<TimeEntry>(query, token);
        timeEntries.AddRange(queryResults.Select(r =>
        {
          long.TryParse(r.RowKey, out long dateTimeLong);
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

    [FunctionName("GetTimeEntries")]
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
        var timeQueryDTO = JsonConvert.DeserializeObject<TimeQueryDto>(data);
        try
        {
          var times = await GetTimes(cloudTable, validPayload.Email, timeQueryDTO);
          return new OkObjectResult(times);
        }
        catch (Exception ex)
        {
          log.LogError(ex.ToString());
        }
      }
      catch (InvalidJwtException ex)
      {
        log.LogError(ex.ToString());
        return new UnauthorizedResult();
      }
      return new BadRequestResult();
    }
  }
}
