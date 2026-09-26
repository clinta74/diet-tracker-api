#nullable enable
using diet_tracker_api.BusinessLayer.Account;
using diet_tracker_api.BusinessLayer.Auth;
using diet_tracker_api.Extensions;
using diet_tracker_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace diet_tracker_api.Controllers;

public record ChangePasswordRequest(string CurrentPassword, string NewPassword);
public record ChangeEmailRequest(string NewEmail);

public record SessionInfo(int Id, DateTime CreatedAt, DateTime ExpiresAt, string? CreatedByIp);

[Authorize]
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AccountController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IPasswordService _passwordService;

    public AccountController(IMediator mediator, IPasswordService passwordService)
    {
        _mediator = mediator;
        _passwordService = passwordService;
    }

    [HttpPut("password")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var credentials = await _mediator.Send(new GetCredentialsByUserId(userId), cancellationToken);

        if (_passwordService.Verify(credentials?.PasswordHash, request.CurrentPassword) == PasswordVerificationResult.Failed)
            return Unauthorized(new { message = "Current password is incorrect." });

        var newHash = _passwordService.Hash(request.NewPassword);
        var success = await _mediator.Send(new ChangePassword(userId, newHash), cancellationToken);

        if (!success)
            return BadRequest(new { message = "Could not update password." });

        return NoContent();
    }

    [HttpPut("email")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult> ChangeEmail([FromBody] ChangeEmailRequest request, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var result = await _mediator.Send(new ChangeEmail(userId, request.NewEmail), cancellationToken);

        if (!result.Success)
            return BadRequest(new { message = result.Error });

        return NoContent();
    }

    [HttpGet("sessions")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<SessionInfo>>> GetSessions(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var tokens = await _mediator.Send(new GetActiveRefreshTokens(userId), cancellationToken);

        var sessions = tokens.Select(t => new SessionInfo(t.Id, t.CreatedAt, t.ExpiresAt, t.CreatedByIp)).ToList();
        return Ok(sessions);
    }

    [HttpDelete("sessions/{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> RevokeSession(int id, CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        var success = await _mediator.Send(new RevokeRefreshTokenById(id, userId), cancellationToken);

        if (!success)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("sessions")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<ActionResult> RevokeAllSessions(CancellationToken cancellationToken)
    {
        var userId = HttpContext.GetUserId();
        await _mediator.Send(new RevokeAllRefreshTokens(userId), cancellationToken);
        return NoContent();
    }
}
