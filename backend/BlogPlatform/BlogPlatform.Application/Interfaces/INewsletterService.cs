using BlogPlatform.Application.DTOs.Community;
using BlogPlatform.Domain.Common;

namespace BlogPlatform.Application.Interfaces;

public interface INewsletterService
{
    Task<ResultModel<SendNewsletterResponse>> InitiateAsync(Guid authorId, SendNewsletterRequest request, CancellationToken ct = default);
    Task<ResultModel<NewsletterSendDto>> ConfirmSendAsync(Guid sendId, Guid authorId, CancellationToken ct = default);
}
