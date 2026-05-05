#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.BusinessLayer.Account;
using diet_tracker_api.BusinessLayer.Admin;
using diet_tracker_api.BusinessLayer.Auth;
using diet_tracker_api.DataLayer;
using diet_tracker_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace diet_tracker_api.Controllers
{
    public record AdminUserDto(
        string UserId,
        string FirstName,
        string LastName,
        string? Email,
        IReadOnlyList<string> Permissions,
        bool HasCredentials
    );

    public record SetUserPermissionsRequest(IReadOnlyList<string> Permissions);
    public record SetUserCredentialsRequest(string Email, string Password);

    [Authorize("admin:users")]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AdminController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly DietTrackerDbContext _dbContext;

        public AdminController(IMediator mediator, IJwtTokenService jwtTokenService, DietTrackerDbContext dbContext)
        {
            _mediator = mediator;
            _jwtTokenService = jwtTokenService;
            _dbContext = dbContext;
        }

        [HttpGet("users")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IReadOnlyList<AdminUserDto>>> GetUsers(CancellationToken cancellationToken)
        {
            var users = await _dbContext.Users
                .AsNoTracking()
                .Include(u => u.Credentials)
                .Include(u => u.Permissions)
                .Select(u => new AdminUserDto(
                    u.UserId,
                    u.FirstName,
                    u.LastName,
                    u.Credentials != null ? u.Credentials.Email : null,
                    u.Permissions.Select(p => p.Permission).ToList(),
                    u.Credentials != null
                ))
                .ToListAsync(cancellationToken);

            return Ok(users);
        }

        [HttpPut("users/{userId}/permissions")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> SetPermissions(string userId, [FromBody] SetUserPermissionsRequest request, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.Users.AnyAsync(u => u.UserId == userId, cancellationToken);
            if (!exists) return NotFound();

            await _mediator.Send(new SetUserPermissions(userId, request.Permissions), cancellationToken);
            return NoContent();
        }

        [HttpPost("users/{userId}/credentials")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> SetCredentials(string userId, [FromBody] SetUserCredentialsRequest request, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.Users.AnyAsync(u => u.UserId == userId, cancellationToken);
            if (!exists) return NotFound();

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            await _mediator.Send(new SetUserCredentials(userId, request.Email, passwordHash), cancellationToken);
            return NoContent();
        }

        [HttpDelete("users/{userId}/sessions")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult> RevokeUserSessions(string userId, CancellationToken cancellationToken)
        {
            var exists = await _dbContext.Users.AnyAsync(u => u.UserId == userId, cancellationToken);
            if (!exists) return NotFound();

            await _mediator.Send(new RevokeAllRefreshTokens(userId), cancellationToken);
            return NoContent();
        }
    }
}
