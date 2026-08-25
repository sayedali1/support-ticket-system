using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketSystem.Application.DTOs;
using SupportTicketSystem.Application.Interfaces;
using System.Security.Claims;

namespace SupportTicketSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        return Ok(await _userService.GetAllAsync());
    }

    [HttpGet("agents")]
    public async Task<IActionResult> GetAgents()
    {
        return Ok(await _userService.GetAgentsAsync());
    }

    [HttpPost]
    public async Task<IActionResult> Create(UserCreateDto dto)
    {
        var result = await _userService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, UserUpdateDto dto)
    {
        var result = await _userService.UpdateAsync(id, dto);
        if (result is null) return NotFound();
        return Ok(result);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(int id)
    {
        var requestingUserId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        var deleted = await _userService.DeleteAsync(id, requestingUserId);
        if (!deleted) return NotFound();
        return NoContent();
    }
}