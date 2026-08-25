using SupportTicketSystem.Application.DTOs;

namespace SupportTicketSystem.Application.Interfaces;

public interface ITicketService
{
    Task<TicketResponseDto?> GetByIdAsync(int ticketId, int requestingUserId, string requestingUserRole);
    Task<PagedResultDto<TicketResponseDto>> GetFilteredAsync(TicketQueryParams query, int requestingUserId, string requestingUserRole);
    Task<TicketResponseDto> CreateAsync(TicketCreateDto dto, int customerId);
    Task<TicketResponseDto?> UpdateAsync(int ticketId, TicketUpdateDto dto, int requestingUserId, string requestingUserRole);

    Task<CommentResponseDto?> AddCommentAsync(int ticketId, CommentCreateDto dto, int requestingUserId, string requestingUserRole);
    Task<List<TimelineEntryDto>?> GetTimelineAsync(int ticketId, int requestingUserId, string requestingUserRole);

    Task<TimeLogResponseDto?> AddTimeLogAsync(int ticketId, TimeLogCreateDto dto, int requestingUserId, string requestingUserRole);
    Task<TicketTimeSummaryDto?> GetTimeSummaryAsync(int ticketId, int requestingUserId, string requestingUserRole);
}