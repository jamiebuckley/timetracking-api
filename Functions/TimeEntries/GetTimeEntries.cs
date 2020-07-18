using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractMechanics.TimeTracking.Models;
using AbstractMechanics.TimeTracking.Models.Dtos;
using AbstractMechanics.TimeTracking.Services;
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
  public class GetTimeEntries
  {
    private readonly TimeEntryService _timeEntryService;

    public GetTimeEntries(TimeEntryService timeEntryService)
    {
      _timeEntryService = timeEntryService;
    }

    [FunctionName("GetTimeEntries")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "timeEntries")] HttpRequest req,
        ILogger log)
    {
      if (!req.Headers.ContainsKey("auth")) return new BadRequestObjectResult("Missing auth header");
      try
      {
        var validPayload = await GoogleJsonWebSignature.ValidateAsync(req.Headers["auth"]);

        if (!req.Query.ContainsKey("startDate") || !req.Query.ContainsKey("endDate"))
        {
          return new BadRequestObjectResult("Query params missing, must pass startDate and endDate");
        }
        
        var startDate = DateTime.Parse(req.Query["startDate"]);
        var endDate = DateTime.Parse(req.Query["endDate"]);
        
        try
        {
          var times = await _timeEntryService.GetTimes(validPayload.Email, startDate, endDate);
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
