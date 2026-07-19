using System.Security.Claims;
using BlogPlatform.API.Extensions;
using BlogPlatform.Application.DTOs.Community;
using BlogPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/newsletter")]
[Authorize(Policy = "RequireBlogger")]
public class NewsletterController(INewsletterService newsletterService) : ControllerBase
{
    [HttpPost("send")]
    public async Task<IActionResult> InitiateSend([FromBody] SendNewsletterRequest request, CancellationToken ct)
    {
        var authorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await newsletterService.InitiateAsync(authorId, request, ct);
        return result.StatusCode == 201
            ? StatusCode(202, result.Data)
            : result.ToActionResult(this);
    }

    [HttpPost("confirm/{sendId:guid}")]
    public async Task<IActionResult> ConfirmSend(Guid sendId, CancellationToken ct)
    {
        var authorId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        return (await newsletterService.ConfirmSendAsync(sendId, authorId, ct)).ToActionResult(this);
    }
}
