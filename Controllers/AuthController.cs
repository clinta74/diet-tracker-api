using System.Threading;
using System.Threading.Tasks;
using diet_tracker_api.BusinessLayer.Admin;
using diet_tracker_api.BusinessLayer.Auth;
using diet_tracker_api.Extensions;
using diet_tracker_api.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace diet_tracker_api.Controllers
{
    public record LoginRequest(string Email, string Password);
    public record RegisterRequest(string FirstName, string LastName, string Email, string Password, int PlanId);
    public record RefreshRequest(string RefreshToken);
    public record RevokeRequest(string RefreshToken);
    public record MigrateRequest(string Email, string NewPassword);

    public record AuthResponse(string AccessToken, string RefreshToken, int ExpiresIn);

    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly IJwtTokenService _jwtTokenService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public AuthController(IMediator mediator, IJwtTokenService jwtTokenService, IHttpContextAccessor httpContextAccessor)
        {
            _mediator = mediator;
            _jwtTokenService = jwtTokenService;
            _httpContextAccessor = httpContextAccessor;
        }

        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]
        public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request, CancellationToken cancellationToken)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);

            var result = await _mediator.Send(
                new RegisterUser(request.FirstName, request.LastName, request.Email, passwordHash, request.PlanId),
                cancellationToken);

            return await BuildAuthResponseAsync(result.UserId, result.Permissions, cancellationToken);
        }

        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
        {
            var result = await _mediator.Send(new LoginUser(request.Email, request.Password), cancellationToken);

            if (result == null)
                return Unauthorized(new { message = "Invalid email or password." });

            return await BuildAuthResponseAsync(result.UserId, result.Permissions, cancellationToken);
        }

        [HttpPost("refresh")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest request, CancellationToken cancellationToken)
        {
            var oldHash = _jwtTokenService.HashToken(request.RefreshToken);
            var newRaw = _jwtTokenService.GenerateRefreshToken();
            var newHash = _jwtTokenService.HashToken(newRaw);
            var newExpiry = DateTime.UtcNow.AddDays(_jwtTokenService.RefreshTokenExpiryDays);
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            var rotateResult = await _mediator.Send(
                new RotateRefreshToken(oldHash, newHash, newExpiry, ip), cancellationToken);

            if (rotateResult == null)
                return Unauthorized(new { message = "Invalid or expired refresh token." });

            var permissions = await _mediator.Send(new GetUserPermissions(rotateResult.UserId), cancellationToken);
            var accessToken = _jwtTokenService.GenerateAccessToken(rotateResult.UserId, permissions);

            return Ok(new AuthResponse(accessToken, newRaw, _jwtTokenService.AccessTokenExpiryMinutes * 60));
        }

        [Authorize]
        [HttpPost("revoke")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult> Revoke([FromBody] RevokeRequest request, CancellationToken cancellationToken)
        {
            var hash = _jwtTokenService.HashToken(request.RefreshToken);
            var revoked = await _mediator.Send(new RevokeRefreshToken(hash), cancellationToken);

            if (!revoked)
                return BadRequest(new { message = "Token not found or already revoked." });

            return NoContent();
        }

        [HttpPost("migrate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<AuthResponse>> Migrate([FromBody] MigrateRequest request, CancellationToken cancellationToken)
        {
            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);

            var result = await _mediator.Send(new MigrateUser(request.Email, passwordHash), cancellationToken);

            if (result == null || result.Error != null)
                return BadRequest(new { message = result?.Error ?? "Migration failed." });

            return await BuildAuthResponseAsync(result.UserId, result.Permissions, cancellationToken);
        }

        private async Task<ActionResult<AuthResponse>> BuildAuthResponseAsync(
            string userId,
            IReadOnlyList<string> permissions,
            CancellationToken cancellationToken)
        {
            var accessToken = _jwtTokenService.GenerateAccessToken(userId, permissions);
            var refreshTokenRaw = _jwtTokenService.GenerateRefreshToken();
            var refreshTokenHash = _jwtTokenService.HashToken(refreshTokenRaw);
            var expiry = DateTime.UtcNow.AddDays(_jwtTokenService.RefreshTokenExpiryDays);
            var ip = _httpContextAccessor.HttpContext?.Connection.RemoteIpAddress?.ToString();

            await _mediator.Send(new CreateRefreshToken(userId, refreshTokenHash, expiry, ip), cancellationToken);

            return Ok(new AuthResponse(accessToken, refreshTokenRaw, _jwtTokenService.AccessTokenExpiryMinutes * 60));
        }
    }
}
