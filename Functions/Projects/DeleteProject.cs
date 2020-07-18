using System;
using System.IO;
using System.Threading.Tasks;
using AbstractMechanics.TimeTracking.Models.Dtos;
using AbstractMechanics.TimeTracking.Services;
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
  public class DeleteProject
  {
    private readonly ProjectService _projectService;

    public DeleteProject(ProjectService _projectService)
    {
      this._projectService = _projectService;
    }

    [FunctionName("DeleteProject")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "delete", Route = "projects")] HttpRequest req,
        ILogger log)
    {
      if (!req.Headers.ContainsKey("auth")) return new BadRequestObjectResult("Missing auth header");
      try
      {
        var validPayload = await GoogleJsonWebSignature.ValidateAsync(req.Headers["auth"]);
        string data = await req.ReadAsStringAsync();
        var projectDeletionRequest = JsonConvert.DeserializeObject<DeleteProjectDto>(data);
        var deletionResult = _projectService.RemoveProject(validPayload.Email, projectDeletionRequest);
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
