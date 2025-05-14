using Microsoft.AspNetCore.Mvc;
using ChatServer.DTOs;
using ChatServer.Services;

namespace ChatServer.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : Controller
    {
        private readonly IUserService _users;
        public UsersController(IUserService users) => _users = users;

        [HttpGet]
        public async Task<ActionResult<List<ContactDto>>> GetAll()
        {
            var all = await _users.GetALLUsersAsync();
            var contacts = all.Select(u => new ContactDto
            {
                Id = u.Id,
                Login = u.Login,
                PhoneNumber = u.PhoneNumber
            }).ToList();
            return Ok(contacts);
        }
    }
}
