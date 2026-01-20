using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using RezerVanaUmv.Models;
using System.Diagnostics;

public class ErrorController : Controller
{
    [Route("Error")]
    public IActionResult GeneralError()
    {
        var exceptionFeature = HttpContext.Features.Get<IExceptionHandlerPathFeature>();
        var model = new ErrorViewModel
        {
            StatusCode = 500,
            ErrorMessage = "Sunucu hatası oluştu.",
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            OriginalPath = exceptionFeature?.Path
        };
        return View("Error", model);
    }

    [Route("Error/{statusCode}")]
    public IActionResult HttpStatusCodeHandler(int statusCode)
    {
        var statusFeature = HttpContext.Features.Get<IStatusCodeReExecuteFeature>();
        var model = new ErrorViewModel
        {
            StatusCode = statusCode,
            ErrorMessage = statusCode switch
            {
                404 => "Sayfa bulunamadı.",
                403 => "Yetkiniz yok.",
                _ => "Beklenmeyen bir hata oluştu."
            },
            RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier,
            OriginalPath = statusFeature?.OriginalPath
        };
        return View("Error", model);
    }
}
