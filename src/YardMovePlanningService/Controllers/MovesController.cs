using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using YardMovePlanningService.Authorization;
using YardMovePlanningService.Data;

namespace YardMovePlanningService.Controllers;

[ApiController]
[Route("api")]
[Authorize]
public sealed class MovesController(YardDbContext dbContext) : ControllerBase
{
    [HttpGet("moves")]
    [Authorize(Policy = Policies.YardRead)]
    public async Task<IActionResult> GetMoves(CancellationToken cancellationToken)
    {
        var jobs = await dbContext.YardMoveJobs.AsNoTracking().OrderBy(item => item.Priority).ToListAsync(cancellationToken);
        return Ok(jobs);
    }

    [HttpGet("moves/pending")]
    [Authorize(Policy = Policies.YardRead)]
    public async Task<IActionResult> GetPendingMoves(CancellationToken cancellationToken)
    {
        var pendingJobs = await dbContext.YardMoveJobs.AsNoTracking()
            .Where(item => item.Status == "pending")
            .OrderBy(item => item.Priority)
            .ToListAsync(cancellationToken);

        return Ok(pendingJobs);
    }

    [HttpPatch("moves/{id:guid}/assign")]
    [Authorize(Policy = Policies.YardWrite)]
    public async Task<IActionResult> AssignMove(Guid id, [FromBody] AssignMoveRequest request, CancellationToken cancellationToken)
    {
        var moveJob = await dbContext.YardMoveJobs.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (moveJob is null)
        {
            return NotFound(new ProblemDetails { Title = "Move not found", Status = StatusCodes.Status404NotFound });
        }

        moveJob.AssignedTo = request.OperatorCode;
        moveJob.Status = "assigned";
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPatch("moves/{id:guid}/priority")]
    [Authorize(Policy = Policies.YardWrite)]
    public async Task<IActionResult> ChangePriority(Guid id, [FromBody] ChangePriorityRequest request, CancellationToken cancellationToken)
    {
        if (!Domain.YardMoveRules.IsValidPriority(request.Priority))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid priority",
                Detail = "Priority must be between 1 and 5.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var moveJob = await dbContext.YardMoveJobs.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (moveJob is null)
        {
            return NotFound(new ProblemDetails { Title = "Move not found", Status = StatusCodes.Status404NotFound });
        }

        moveJob.Priority = request.Priority;
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPatch("moves/{id:guid}/complete")]
    [Authorize(Policy = Policies.YardWrite)]
    public async Task<IActionResult> CompleteMove(Guid id, CancellationToken cancellationToken)
    {
        var moveJob = await dbContext.YardMoveJobs.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (moveJob is null)
        {
            return NotFound(new ProblemDetails { Title = "Move not found", Status = StatusCodes.Status404NotFound });
        }

        if (!Domain.YardMoveRules.CanBeCompleted(moveJob.Status))
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid move state",
                Detail = "Only pending, assigned, or rescheduled jobs can be completed.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        moveJob.Status = "completed";
        await dbContext.SaveChangesAsync(cancellationToken);

        return NoContent();
    }

    [HttpPatch("moves/{id:guid}/reschedule")]
    [Authorize(Policy = Policies.YardWrite)]
    public async Task<IActionResult> RescheduleMove(Guid id, [FromBody] RescheduleMoveRequest request, CancellationToken cancellationToken)
    {
        if (request.ScheduledAtUtc <= DateTime.UtcNow)
        {
            return BadRequest(new ProblemDetails
            {
                Title = "Invalid schedule date",
                Detail = "ScheduledAtUtc must be in the future.",
                Status = StatusCodes.Status400BadRequest
            });
        }

        var moveJob = await dbContext.YardMoveJobs.FirstOrDefaultAsync(item => item.Id == id, cancellationToken);
        if (moveJob is null)
        {
            return NotFound(new ProblemDetails { Title = "Move not found", Status = StatusCodes.Status404NotFound });
        }

        moveJob.ScheduledAtUtc = request.ScheduledAtUtc;
        if (moveJob.Status == "completed")
        {
            moveJob.Status = "rescheduled";
        }

        await dbContext.SaveChangesAsync(cancellationToken);
        return NoContent();
    }

    [HttpGet("planning/status")]
    [Authorize(Policy = Policies.YardRead)]
    public async Task<IActionResult> GetPlanningStatus(CancellationToken cancellationToken)
    {
        var totalJobs = await dbContext.YardMoveJobs.CountAsync(cancellationToken);
        var completedJobs = await dbContext.YardMoveJobs.CountAsync(item => item.Status == "completed", cancellationToken);
        var pendingJobs = await dbContext.YardMoveJobs.CountAsync(item => item.Status == "pending", cancellationToken);

        return Ok(new
        {
            totalJobs,
            completedJobs,
            pendingJobs,
            completionRate = totalJobs == 0 ? 0 : Math.Round((decimal)completedJobs / totalJobs * 100, 2)
        });
    }
}

public sealed class AssignMoveRequest
{
    public string OperatorCode { get; set; } = string.Empty;
}

public sealed class ChangePriorityRequest
{
    public int Priority { get; set; }
}

public sealed class RescheduleMoveRequest
{
    public DateTime ScheduledAtUtc { get; set; }
}
