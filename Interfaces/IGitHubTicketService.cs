namespace Ava.API.Interfaces;
public interface IGitHubTicketService
{
    Task<List<GitHubTicket>> GetAllOpenTicketsAsync();
    Task<GitHubTicket?> GetTicketAsync(int ticketNumber);
    Task<List<GitHubTicket>> GetTicketsByCategoryAsync(string category);
    Task<List<GitHubTicket>> GetTicketsByPriorityAsync(string priority);
    Task<List<GitHubComment>> GetCommentsAsync(int ticketNumber);
    Task<bool> AddCommentAsync(int ticketNumber, string comment);
    Task<bool> CloseTicketAsync(int ticketNumber);
    Task<bool> ReopenTicketAsync(int ticketNumber);
    Task<bool> ReplaceTagAsync(int ticketNumber, string fromTag, string toTag);
    Task<bool> DeleteTicketAsync(int ticketNumber);
    Task<bool> ReassignTicketAsync(int ticketNumber);
    Task<bool> EnsureLabelExistsAsync(string labelName, string color = "B60205");
    Task<bool> CreateInternalTicketAsync(InternalSupportTicket internalSupportTicket);
}
