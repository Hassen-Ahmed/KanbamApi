
using KanbamApi.Models;
using KanbamApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace KanbamApi.Controllers;
[ApiController]
[Route("api/[controller]")]
public class ListsController : ControllerBase {
private readonly ListsService _listsService;
    public ListsController(ListsService listsService) =>
        _listsService = listsService;

   [HttpGet]
    public async Task<List<List>> Get() =>
        await _listsService.GetAsync();
}