using Microsoft.AspNetCore.Authentication;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using UsersApi.Models;

namespace UsersApi.Handlers
{
    public class BasicAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        private readonly AppDbContext _context;

        public BasicAuthenticationHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder, ISystemClock clock, AppDbContext context)
            : base(options, logger, encoder, clock)
        {
            _context = context;
        }

        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
            {
                return AuthenticateResult.Fail("Не найден заголовок авторизации");
            }

            var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
            if (authHeader.Scheme != "Basic")
            {
                return AuthenticateResult.Fail("Неверная схема авторизации");
            }
            var credentialBytes = Convert.FromBase64String(authHeader.Parameter);
            var credentials = Encoding.UTF8.GetString(credentialBytes).Split(':');
            var login = credentials[0];
            var pass = credentials[1];

            var user = await _context.Users.SingleOrDefaultAsync(n=>n.Login == login && n.Password == pass);
            if (user == null)
            {
                return AuthenticateResult.Fail("Неверный логин или пароль");
            }
            var role = _context.UserGroups.SingleOrDefault(n => n.Id == user.UserGroupId);
            if (role == null)
            {
                return AuthenticateResult.Fail("У пользователя не задана роль");
            }
            var claims = new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new Claim(ClaimTypes.Name, user.Login),
            new Claim(ClaimTypes.Role, role.Code)
        };
            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            return AuthenticateResult.Success(ticket);
        }
    }
}
