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

namespace AbstractMechanics.TimeTracking.Functions.Projects
{
  public class CreateProject
  {
    private readonly ProjectService _projectService;

    public CreateProject(ProjectService projectService)
    {
      _projectService = projectService;
    }

    [FunctionName("CreateProject")]
    public async Task<IActionResult> Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "projects")] HttpRequest req,
        ILogger log)
    {

      if (!req.Headers.ContainsKey("auth")) return new BadRequestObjectResult("Missing auth header");
      try
      {
        var validPayload = await GoogleJsonWebSignature.ValidateAsync(req.Headers["auth"]);
        string data = await req.ReadAsStringAsync();
        var projectCreationRequest = JsonConvert.DeserializeObject<CreateProjectDto>(data);
        await _projectService.InsertProject(validPayload.Email, projectCreationRequest);
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
