using Hangfire;
using Microsoft.AspNetCore.Mvc;

namespace ApiForLongRunningTasks.Controllers
{
    [ApiController]
    [Route("excel-upload-controller")]
    public class ExcelUploadController : ControllerBase
    {
        private readonly ILogger<ExcelUploadController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public ExcelUploadController(ILogger<ExcelUploadController> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        /// <summary>
        /// Inserts uploading excel file's content asynchronously to database using Hangfire background worker.
        /// </summary>
        /// <param name="file">Should be excel file with "xls, xlsx" extensions.</param>
        /// <returns>If the file is a valid excel file and the job is added, it returns successful state.</returns>
        [HttpPost("upload-file")]
        public async Task<IActionResult> UploadExcelFile(IFormFile file)
        {
            try
            {
                if (!VerifyExcelFile(file, out var actionResult))
                    return actionResult;

                var filePath = Path.Combine(Directory.GetCurrentDirectory(), "Unprocessed", file.FileName);

                await using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await file.CopyToAsync(stream);
                }

                BackgroundJob.Enqueue(() => new ExcelProcessor(_serviceProvider).ProcessExcelFile(filePath));

                return Ok("File uploaded successfully.");
            }
            catch (Exception exception)
            {
                _logger.Log(LogLevel.Error, exception.Message);
                return Problem("Unknown error occurred.");
            }
        }

        private bool VerifyExcelFile(IFormFile file, out IActionResult actionResult)
        {
            actionResult = null;

            if (file.Length == 0)
            {
                var message = $"{file.FileName} is Empty.";
                _logger.Log(LogLevel.Warning, message);
                {
                    actionResult = BadRequest(message);
                    return false;
                }
            }

            if (Path.GetExtension(file.FileName).ToLower() == ".xls" ||
                Path.GetExtension(file.FileName).ToLower() == ".xlsx") 
                return true;

            {
                var message = $"{file.FileName} is not an excel file.";
                _logger.Log(LogLevel.Warning, message);
                {
                    actionResult = BadRequest(message);
                    return false;
                }
            }

        }
    }
}