using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

using UsersApi.Models;

namespace UsersApi.Controllers
{
	[Route("api/[controller]")]
	[ApiController]
	[Produces("application/json")]
	[Authorize]
	public class UsersController : Controller
	{
		private readonly IUsersApiDbContext _context;
        private static object _lock = new object();
        public UsersController(IUsersApiDbContext context)
		{
			_context = context;
		}

		[HttpGet]
		public async Task<ActionResult<IEnumerable<User>>> GetUsers()
		{
			if (_context.Users == null)
			{
				return NotFound();
			}

			return await _context.Users.ToListAsync();
		}

		[HttpGet("{id}")]
		public async Task<ActionResult<User>> GetUser(int id)
		{
			if (_context.Users == null)
			{
				return NotFound();
			}
			var user = await _context.Users.FindAsync(id);

			if (user == null)
			{
				return NotFound();
			}

			return user;
		}

		[HttpGet("{page}/{pageSize}")]
		public async Task<ActionResult<IEnumerable<User>>> GetUsersByPage(int page, int pageSize)
		{
			if (_context.Users == null)
			{
				return NotFound();
			}

			return await _context.Users.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();
		}

		[HttpPost]
		[Route("/api/AddUser")]
        public async Task<ActionResult> AddUser(UserRequestModel rawUser)
        {
            if (_context.Users == null)
            {
                return NotFound();
            }
			if (_context.Users.Any(n=> n.Login== rawUser.Login))
			{
				return BadRequest("Уже существует пользователь с заданным логином");
			}
			var user = new User();
			lock (_lock)
			{
                if (_context.Users.Any(n => n.Login == rawUser.Login))
                {
                    return BadRequest("Уже существует пользователь с заданным логином");
                }
				var delay = 5000;
				user = rawUser.ConvertToUser();
				var groupId = _context.GetGroupIdByCode(rawUser.UserGroup).Result;
				if (groupId == 0)
				{
                    return BadRequest("Не найдена указанная роль для пользователя");
                }
				if (_context.IsAdmin(groupId) && _context.Users.Any(n=>n.UserGroupId == groupId))
				{
					return BadRequest("Админ уже существует");
				}
                user.UserGroupId = groupId;
				Task.Delay(delay).Wait();
				_context.Users.Add(user);
				_context.SaveChanges();
            }
            var stateId = await _context.GetStateIdByCode(UserState.Active);
            if (stateId == 0)
            {
                return Problem("Не найден активный статус в базе");
            }
            user.UserStateId = stateId;
			await _context.SaveChangesAsync();
            return Ok(user);
        }



		[HttpDelete("{id}")]
		public async Task<IActionResult> DeleteUser(int id)
		{
			if (_context.Users == null)
			{
				return NotFound();
			}
			var user = await _context.Users.FindAsync(id);
			if (user == null)
			{
				return NotFound();
			}
			var stateId = await _context.GetStateIdByCode(UserState.Blocked);
			if (stateId == 0)
			{
				return Problem("Не найден статус заблокированного пользователя в базе");
			}
			if (user.UserStateId != stateId)
			{
				user.UserStateId = stateId;
				await _context.SaveChangesAsync();
			}
			return Ok();
		}
	}
}
