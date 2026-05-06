using Microsoft.AspNetCore.Mvc;
using UseCases;
using UseCases.ExtractTableHeaders;
using UseCases.GetPreview;
using UseCases.UploadTemplate;

namespace Presentation.Endpoints;

[ApiController]
[Route("api")]
public class EmailPreparationEndpoints(
    IUploadTemplateRequestHandler uploadTemplateRequestHandler,
    IProcessEmailCreationRequestHandler processEmailCreationRequestHandler,
    IExtractTableHeadersRequestHandler extractTableHeadersRequestHandler,
    IGetPreviewRequestHandler getPreviewRequestHandler,
    ISendRequestHandler sendRequestHandler)
    : ControllerBase
{
    /// <summary>
    /// Загрузка html шаблона письма, для обнаружения подстановочных переменных.
    /// </summary>
    /// <param name="request">html шаблон.</param>
    /// <returns>Список названий подстановочных переменных, обнаруженных в шаблоне.</returns>
    [HttpPost("UploadTemplate")]
    public IActionResult UploadTemplate([FromForm] UploadTemplateRequest request)
    {
        try
        {
            var response = uploadTemplateRequestHandler.Handle(request);
            return Ok(response);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return BadRequest("Unknown exception");
        }
    }

    [HttpPost("ProcessEmailCreation")]
    public IActionResult ProcessEmailCreation([FromForm] ProcessEmailCreationRequest request)
    {
        return Ok(processEmailCreationRequestHandler.Handle(request));
    }

    /// <summary>
    /// Получение всех заголовков столбцов из таблицы.
    /// Заголовки берутся из первой не пустой строки таблицы.
    /// Формат таблицы .xlsx.
    /// </summary>
    /// <param name="request">.xlsx таблица.</param>
    /// <returns>Заголовки в виде списка строк, если нет заголовков то код 422.</returns>
    [HttpPost("ExtractTableHeaders")]
    public IActionResult ExtractTableHeaders([FromForm] ExtractTableHeadersRequest request)
    {
        try
        {
            var response = extractTableHeadersRequestHandler.Handle(request);
            return response.headers.Count == 0
                ? UnprocessableEntity("There are no headers in the table")
                : Ok(response.headers);
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
        catch (Exception)
        {
            return BadRequest("Unknown exception");
        }
    }
    
    /// <summary>
    /// Получение определенного числа писем для предпросмотра на сайте.
    /// </summary>
    /// <param name="request">.xlsx таблица,
    /// .html шаблон.
    /// Начальная строка таблицы.
    /// Количество писем которые нужно сгенерировать.
    /// Переменные шаблона со столбцами таблицы.</param>
    /// <returns>Список писем с адресатами и номер строки следующей за той, что была обработана последней.</returns>
    [HttpPost("GetPreview")]
    public IActionResult GetPreview([FromForm] GetPreviewRequest request)
    {
        return Ok(getPreviewRequestHandler.Handle(request));
    }
    
    
    [HttpPost("Send")]
    public async Task<IActionResult> Send([FromForm] SendRequest request)
    {
        try
        {
            return Ok(await sendRequestHandler.Handle(request));
        }
        catch (ArgumentException ex)
        {
            return BadRequest(ex.Message);
        }
    }
}
