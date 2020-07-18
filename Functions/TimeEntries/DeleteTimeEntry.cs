using System;
using System.IO;
using System.Threading.Tasks;
using AbstractMechanics.TimeTracking.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AbstractMechanics.TimeTracking.Functions.TimeEntries
{
    public class DeleteTimeEntry
    {
        private readonly TimeEntryService _timeEntryService;

        public DeleteTimeEntry(TimeEntryService timeEntryService)
        {
            _timeEntryService = timeEntryService;
        }
        
        [FunctionName("DeleteTimeEntry")]
        public async Task<IActionResult> RunAsync(
            [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "timeEntries")]
            HttpRequest req, ILogger log)
        {
            if (!req.Headers.ContainsKey("auth")) return new BadRequestObjectResult("Missing auth header");
            try
            {
                var validPayload = await GoogleJsonWebSignature.ValidateAsync(req.Headers["auth"]);
                var timeString = req.Query["dateTime"];
                var date = DateTime.Parse(timeString);
                var keyId = req.Query["key"];
                var times = await _timeEntryService.DeleteTimeEntry(validPayload.Email, date, keyId);
                return new OkObjectResult(times);
            }
            catch (InvalidJwtException ex)
            {
                log.LogError(ex.ToString());
                return new UnauthorizedResult();
            }
        }
    }
}