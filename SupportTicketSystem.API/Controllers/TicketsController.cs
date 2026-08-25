using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SupportTicketSystem.Application.DTOs;
using SupportTicketSystem.Application.Interfaces;

namespace SupportTicketSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize] 
public class TicketsController : ControllerBase
{
    private readonly ITicketService _ticketService;

    public TicketsController(ITicketService ticketService)
    {
        _ticketService = ticketService;
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _ticketService.GetByIdAsync(id, GetUserId(), GetUserRole());
        if (result is null)
            return NotFound(); 

        return Ok(result);
    }

    [HttpGet]
    public async Task<IActionResult> GetFiltered([FromQuery] TicketQueryParams query)
    {
        var result = await _ticketService.GetFilteredAsync(query, GetUserId(), GetUserRole());
        return Ok(result);
    }


    [HttpPost]
    [Authorize(Roles = "Customer")] 
    public async Task<IActionResult> Create(TicketCreateDto dto)
    {
        var result = await _ticketService.CreateAsync(dto, GetUserId());
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(int id, TicketUpdateDto dto)
    {
        var result = await _ticketService.UpdateAsync(id, dto, GetUserId(), GetUserRole());
        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(int id, CommentCreateDto dto)
    {
        var result = await _ticketService.AddCommentAsync(id, dto, GetUserId(), GetUserRole());
        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("{id}/timeline")]
    public async Task<IActionResult> GetTimeline(int id)
    {
        var result = await _ticketService.GetTimelineAsync(id, GetUserId(), GetUserRole());
        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpPost("{id}/timelogs")]
    public async Task<IActionResult> AddTimeLog(int id, TimeLogCreateDto dto)
    {
        var result = await _ticketService.AddTimeLogAsync(id, dto, GetUserId(), GetUserRole());
        if (result is null)
            return NotFound();

        return Ok(result);
    }

    [HttpGet("{id}/timelogs")]
    public async Task<IActionResult> GetTimeSummary(int id)
    {
        var result = await _ticketService.GetTimeSummaryAsync(id, GetUserId(), GetUserRole());
        if (result is null)
            return NotFound();

        return Ok(result);
    }
    private int GetUserId()
    {
        var idClaim = User.FindFirst(ClaimTypes.NameIdentifier) ?? User.FindFirst("sub");
        return int.Parse(idClaim!.Value);
    }

    private string GetUserRole()
    {
        return User.FindFirst(ClaimTypes.Role)!.Value;
    }
}