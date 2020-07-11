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

namespace AbstractMechanics.TimeTracking.Function
{
    public static class ListProjects
    {
        [FunctionName("ListProjects")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.Log(LogLevel.Error, "Processing request");
            if (!req.Headers.ContainsKey("auth")) {
                log.Log(LogLevel.Error, "No auth header");
                return new UnauthorizedResult();
            }
            var authorisation = req.Headers["auth"];

            try {
                var validPayload = await GoogleJsonWebSignature.ValidateAsync(authorisation);
                string responseMessage = "OK";
                return new OkObjectResult(responseMessage);
            } 
            catch(InvalidJwtException ex) {
                log.LogError(ex.ToString());
                return new UnauthorizedResult();
            }
        }
    }
}
