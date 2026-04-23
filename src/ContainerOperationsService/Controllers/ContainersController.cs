using ContainerOperationsService.Authorization;
using ContainerOperationsService.Data;
using ContainerOperationsService.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace ContainerOperationsService.Controllers;

[ApiController]
[Route("api/containers")]
[Authorize]
public sealed class ContainersController(ContainerDbContext dbContext) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Policies.ContainersRead)]
    [ProducesResponseType(typeof(IEnumerable<ContainerSummaryResponse>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAll(CancellationToken cancellationToken)
    {
        var containers = await dbContext.Containers.AsNoTracking()
            .OrderBy(item => item.ContainerNumber)
            .Select(item => new ContainerSummaryResponse(item.Id, item.ContainerNumber, item.Status, item.LastUpdatedUtc))
            .ToListAsync(cancellationToken);

        return Ok(containers);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Policies.ContainersRead)]
    [ProducesResponseType(typeof(ContainerDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var container = await dbContext.Containers.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (container is null)
        {
            return NotFound(new ProblemDetails { Title = "Container not found", Status = StatusCodes.Status404NotFound });
        }

        var events = await dbContext.Events.AsNoTracking()
            .Where(item => item.ContainerUnitId == id)
            .OrderByDescending(item => item.OccurredAtUtc)
            .Select(item => new ContainerEventResponse(item.EventType, item.Notes, item.OccurredAtUtc))
            .ToListAsync(cancellationToken);

        return Ok(new ContainerDetailResponse(container.Id, container.ContainerNumber, container.Status, container.LastUpdatedUtc, events));
    }

    [HttpPatch("{id:guid}/status")]
    [Authorize(Policy = Policies.ContainersWrite)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ChangeStatus(Guid id, [FromBody] UpdateContainerStatusRequest request, CancellationToken cancellationToken)
    {
        if (!ContainerStatusRules.IsValidStatus(request.NewStatus))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid status",
                Detail = "Status must be one of: inbound, outbound, hold, customs-release, loaded, unloaded.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var container = await dbContext.Containers.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (container is null)
        {
            return NotFound(new ProblemDetails { Title = "Container not found", Status = StatusCodes.Status404NotFound });
        }

        container.Status = request.NewStatus;
        container.LastUpdatedUtc = DateTime.UtcNow;
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPost("{id:guid}/events")]
    [Authorize(Policy = Policies.ContainersWrite)]
    [ProducesResponseType(typeof(object), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ProblemDetails), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RegisterEvent(Guid id, [FromBody] RegisterContainerEventRequest request, CancellationToken cancellationToken)
    {
        var containerExists = await dbContext.Containers.AnyAsync(item => item.Id == id, cancellationToken);
        if (!containerExists)
        {
            return NotFound(new ProblemDetails { Title = "Container not found", Status = StatusCodes.Status404NotFound });
        }

        var containerEvent = new ContainerOperationEvent
        {
            Id = Guid.NewGuid(),
            ContainerUnitId = id,
            EventType = request.EventType,
            Notes = request.Notes,
            OccurredAtUtc = DateTime.UtcNow
        };

        dbContext.Events.Add(containerEvent);
        await dbContext.SaveChangesAsync(cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id }, new { containerEvent.Id });
    }
}

public sealed record ContainerSummaryResponse(Guid Id, string ContainerNumber, string Status, DateTime LastUpdatedUtc);
public sealed record ContainerEventResponse(string EventType, string Notes, DateTime OccurredAtUtc);
public sealed record ContainerDetailResponse(Guid Id, string ContainerNumber, string Status, DateTime LastUpdatedUtc, IReadOnlyCollection<ContainerEventResponse> Events);

public sealed class UpdateContainerStatusRequest
{
    public string NewStatus { get; set; } = string.Empty;
}

public sealed class RegisterContainerEventRequest
{
    public string EventType { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
}
