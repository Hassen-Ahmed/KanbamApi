

using KanbamApi.Models;
using KanbamApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace KanbamApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase {
private readonly UsersService _usersService;
    public UsersController(UsersService usersService) =>
        _usersService = usersService;

   [HttpGet]
    public async Task<List<User>> Get() =>
        await _usersService.GetAllUserAsync();
}