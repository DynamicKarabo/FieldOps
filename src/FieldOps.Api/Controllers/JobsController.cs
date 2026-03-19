using FieldOps.Application.Jobs.Commands;
using FieldOps.Application.Jobs.Queries;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace FieldOps.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
public class JobsController : ControllerBase
{
    private readonly IMediator _mediator;

    public JobsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost]
    public async Task<ActionResult<Guid>> Create(CreateJobCommand command)
    {
        var jobId = await _mediator.Send(command);
        return CreatedAtAction(nameof(GetById), new { id = jobId }, jobId);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<JobDto>> GetById(Guid id)
    {
        var job = await _mediator.Send(new GetJobByIdQuery(id));
        if (job == null)
        {
            return NotFound();
        }

        return Ok(job);
    }

    [HttpPost("{id:guid}/assign")]
    public async Task<IActionResult> Assign(Guid id, [FromBody] AssignJobCommandRequest request)
    {
        await _mediator.Send(new AssignJobCommand(id, request.TechnicianId, request.TriggeredBy));
        return NoContent();
    }

    [HttpPost("{id:guid}/acknowledge")]
    public async Task<IActionResult> Acknowledge(Guid id, [FromBody] JobActionRequest request)
    {
        await _mediator.Send(new AcknowledgeJobCommand(id, request.TriggeredBy));
        return NoContent();
    }

    [HttpPost("{id:guid}/en-route")]
    public async Task<IActionResult> EnRoute(Guid id, [FromBody] JobActionRequest request)
    {
        await _mediator.Send(new EnRouteJobCommand(id, request.TriggeredBy));
        return NoContent();
    }

    [HttpPost("{id:guid}/on-site")]
    public async Task<IActionResult> OnSite(Guid id, [FromBody] JobActionRequest request)
    {
        await _mediator.Send(new OnSiteJobCommand(id, request.TriggeredBy, request.CorrelationId));
        return NoContent();
    }

    [HttpPost("{id:guid}/close")]
    public async Task<IActionResult> Close(Guid id, [FromBody] CloseJobCommandRequest request)
    {
        await _mediator.Send(new CloseJobCommand(id, request.ResolutionNotes, request.TriggeredBy));
        return NoContent();
    }

    [HttpPost("{id:guid}/escalate")]
    public async Task<IActionResult> Escalate(Guid id, [FromBody] EscalateJobCommandRequest request)
    {
        await _mediator.Send(new EscalateJobCommand(id, request.Reason, request.TriggeredBy));
        return NoContent();
    }
}

public record AssignJobCommandRequest(Guid TechnicianId, string TriggeredBy);
public record JobActionRequest(string TriggeredBy, Guid? CorrelationId = null);
public record CloseJobCommandRequest(string ResolutionNotes, string TriggeredBy);
public record EscalateJobCommandRequest(string Reason, string TriggeredBy);
