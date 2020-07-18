using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AbstractMechanics.TimeTracking.Models.Dtos;
using AbstractMechanics.TimeTracking.Services;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Cosmos.Table;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;

namespace AbstractMechanics.TimeTracking.Functions.Projects
{
  public class ListProjects
  {
    private readonly ProjectService _projectService;

    public ListProjects(ProjectService projectService)
    {
      _projectService = projectService;
    }


    [FunctionName("ListProjects")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "projects")] HttpRequest req,
        ILogger log)
    {
      if (!req.Headers.ContainsKey("auth")) return new BadRequestObjectResult("Missing auth header");
      try
      {
        var validPayload = await GoogleJsonWebSignature.ValidateAsync(req.Headers["auth"]);
        var projects = await _projectService.GetProjects(validPayload.Email);
        return new OkObjectResult(projects);
      }
      catch (InvalidJwtException ex)
      {
        log.LogError(ex.ToString());
        return new UnauthorizedResult();
      }
    }
  }
}
