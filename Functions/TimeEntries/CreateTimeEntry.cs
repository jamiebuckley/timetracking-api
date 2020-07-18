using System;
using System.Threading.Tasks;
using AbstractMechanics.TimeTracking.Models.Dtos;
using AbstractMechanics.TimeTracking.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AbstractMechanics.TimeTracking.Functions.TimeEntries
{
  public class CreateTimeEntry
  {
    private readonly TimeEntryService _timeEntryService;

    public CreateTimeEntry(TimeEntryService timeEntryService)
    {
      _timeEntryService = timeEntryService;
    }

    [FunctionName("CreateTimeEntry")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "timeEntries")] HttpRequest req,
        ILogger log)
    {
      if (!req.Headers.ContainsKey("auth")) return new BadRequestObjectResult("Missing auth header");
      try
      {
        var validPayload = await GoogleJsonWebSignature.ValidateAsync(req.Headers["auth"]);
        string data = await req.ReadAsStringAsync();
        var timeEntryCreationRequest = JsonConvert.DeserializeObject<TimeEntryDto>(data);
        try {
        await _timeEntryService.InsertEntry(validPayload.Email, timeEntryCreationRequest);
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
