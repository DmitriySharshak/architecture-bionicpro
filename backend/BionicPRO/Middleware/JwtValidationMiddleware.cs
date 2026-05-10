using System.IdentityModel.Tokens.Jwt;

namespace BionicPRO.Middleware
{
    public class JwtValidationMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<JwtValidationMiddleware> _logger;

        public JwtValidationMiddleware(RequestDelegate next, ILogger<JwtValidationMiddleware> logger)
        {
            _next   = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            var token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();

            if (!string.IsNullOrEmpty(token))
            {
                try
                {
                    var tokenHandler = new JwtSecurityTokenHandler();
                    var jwtToken     = tokenHandler.ReadJwtToken(token);

                    // Логируем информацию о пользователе для аудита
                    var userId = jwtToken.Claims.FirstOrDefault(c => c.Type == "preferred_username")?.Value;
                    var roles  = jwtToken.Claims.Where(c => c.Type == "realm_access").Select(c => c.Value);

                    _logger.LogInformation("Request from user {UserId} with roles {Roles} to {Path}",
                        userId, string.Join(", ", roles), context.Request.Path);
                }
                catch (Exception ex)
                {
                    _logger.LogWarning(ex, "Failed to validate JWT token");
                }
            }

            await _next(context);
        }
    }
}
