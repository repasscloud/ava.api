namespace Ava.API.Controllers.GitHub;

[ApiController]
[Route("api/v1/github/issues")]
public class GitHubIssuesController : ControllerBase
{
    private readonly IGitHubTicketService _ticketService;
    private readonly IJwtTokenService _jwtTokenService;

    public GitHubIssuesController(IGitHubTicketService ticketService, IJwtTokenService jwtTokenService)
    {
        _ticketService = ticketService;
        _jwtTokenService = jwtTokenService;
    }

    [ResponseCache(NoStore = true, Duration = 0, Location = ResponseCacheLocation.None)]
    [HttpGet("open")]
    public async Task<IActionResult> GetAllOpenTickets()
    {
        var (isValid, error) = await ValidateBearerTokenAsync();
        if (!isValid)
        {
            return error!;
        }

        var tickets = await _ticketService.GetAllOpenTicketsAsync();
        return Ok(tickets);
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetTicket(int id)
    {
        var ticket = await _ticketService.GetTicketAsync(id);
        return ticket is null ? NotFound() : Ok(ticket);
    }

    [HttpGet("{id}/comments")]
    public async Task<IActionResult> GetComments(int id)
    {
        var comments = await _ticketService.GetCommentsAsync(id);
        return Ok(comments);
    }

    [HttpPost("{id}/comments")]
    public async Task<IActionResult> AddComment(int id, [FromBody] string comment)
    {
        var (isValid, error) = await ValidateBearerTokenAsync();
        if (!isValid)
        {
            return error!;
        }

        var result = await _ticketService.AddCommentAsync(id, comment);
        return result ? Ok() : BadRequest("Unable to add comment.");
    }

    [HttpPost("{id}/close")]
    public async Task<IActionResult> CloseTicket(int id)
    {
        var (isValid, error) = await ValidateBearerTokenAsync();
        if (!isValid)
        {
            return error!;
        }

        var result = await _ticketService.CloseTicketAsync(id);
        return result ? Ok() : BadRequest("Unable to close ticket.");
    }

    [HttpPost("{id}/reopen")]
    public async Task<IActionResult> ReopenTicket(int id)
    {
        var result = await _ticketService.ReopenTicketAsync(id);
        return result ? Ok() : BadRequest("Unable to reopen ticket.");
    }

    [HttpPost("{id}/replace-tag")]
    public async Task<IActionResult> ReplaceTag(int id, [FromBody] TagReplaceRequest request)
    {
        var (isValid, error) = await ValidateBearerTokenAsync();
        if (!isValid)
        {
            return error!;
        }

        var result = await _ticketService.ReplaceTagAsync(id, request.From, request.To);
        return result ? Ok() : BadRequest("Unable to replace tag.");
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteTicket(int id)
    {
        var (isValid, error) = await ValidateBearerTokenAsync();
        if (!isValid)
        {
            return error!;
        }

        var result = await _ticketService.DeleteTicketAsync(id);
        return result ? Ok() : BadRequest("Unable to delete ticket.");
    }

    [HttpGet("filter/category/{tag}")]
    public async Task<IActionResult> GetTicketsByCategory(string tag)
    {
        var (isValid, error) = await ValidateBearerTokenAsync();
        if (!isValid)
        {
            return error!;
        }

        var tickets = await _ticketService.GetTicketsByCategoryAsync(tag);
        return Ok(tickets);
    }

    [HttpGet("filter/priority/{tag}")]
    public async Task<IActionResult> GetTicketsByPriority(string tag)
    {
        var (isValid, error) = await ValidateBearerTokenAsync();
        if (!isValid)
        {
            return error!;
        }

        var tickets = await _ticketService.GetTicketsByPriorityAsync(tag);
        return Ok(tickets);
    }

    [HttpPost("{id}/reassign")]
    public async Task<IActionResult> ReassignTicket(int id)
    {
        var (isValid, error) = await ValidateBearerTokenAsync();
        if (!isValid)
        {
            return error!;
        }

        var result = await _ticketService.ReassignTicketAsync(id);
        return result ? Ok() : BadRequest("Unable to reassign ticket.");
    }

    [HttpPost("internal")]
    public async Task<IActionResult> CreateInternalIssue([FromBody] InternalSupportTicket supportTicket)
    {
        var (isValid, error) = await ValidateBearerTokenAsync();
        if (!isValid)
        {
            return error!;
        }

        var result = await _ticketService.CreateInternalTicketAsync(supportTicket);
        return result ? Ok() : BadRequest("Unable to reassign ticket.");
    }

    private async Task<(bool IsValid, IActionResult? ErrorResult)> ValidateBearerTokenAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var authHeader) || string.IsNullOrWhiteSpace(authHeader))
            return (false, Unauthorized("Missing Authorization header"));

        var bearerToken = authHeader.ToString();
        if (!bearerToken.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
            return (false, Unauthorized("Invalid token format"));

        bearerToken = bearerToken["Bearer ".Length..].Trim();

        bool tokenValid = await _jwtTokenService.ValidateTokenAsync(jwtToken: bearerToken);
        if (!tokenValid)
            return (false, Unauthorized("Invalid or expired token"));

        return (true, null);
    }
}

public class TagReplaceRequest
{
    public string From { get; set; } = string.Empty;
    public string To { get; set; } = string.Empty;
}

