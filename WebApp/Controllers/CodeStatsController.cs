using Microsoft.AspNetCore.Mvc;
using WebApp.Services;

namespace WebApp.Controllers;

/// <summary>
/// Visar kodstatistik för projektet (antal filer och kodrader per filtyp).
/// </summary>
public sealed class CodeStatsController : Controller
{
    private readonly CodeStatsService _codeStats;

    public CodeStatsController(CodeStatsService codeStats)
    {
        _codeStats = codeStats;
    }

    [HttpGet]
    public IActionResult Index()
    {
        var vm = _codeStats.GetStats();
        return View("Index", vm);
    }
}
