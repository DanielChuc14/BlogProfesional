using BlogPlatform.API.Extensions;
using BlogPlatform.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace BlogPlatform.API.Controllers;

[ApiController]
[Route("api/languages")]
public class LanguagesController(ILanguageService languageService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetActiveLanguages(CancellationToken ct)
        => (await languageService.GetActiveLanguagesAsync(ct)).ToActionResult(this);

    [HttpGet("{code}/translations")]
    public async Task<IActionResult> GetTranslation(string code, CancellationToken ct)
    {
        var result = await languageService.GetTranslationAsync(code, ct);
        if (!result.IsSuccess)
            return result.ToActionResult(this);

        return Content(result.Data!, "application/json");
    }
}
