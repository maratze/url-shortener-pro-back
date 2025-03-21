using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System.Drawing;
using System.Drawing.Imaging;

namespace UrlShortenerPro.Api.Controllers;

[ApiController]
[Route("api/qr-code")]
public class QrCodeController(ILogger<QrCodeController> logger) : ControllerBase
{
    /// <summary>
    /// Generates a QR code image from the provided data string
    /// </summary>
    /// <param name="data">The data to encode in the QR code</param>
    /// <returns>QR code image</returns>
    [HttpGet]
    [Route("generate")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GenerateQrCode([FromQuery] string data)
    {
        try
        {
            if (string.IsNullOrEmpty(data))
            {
                logger.LogWarning("QR code generation failed: No data provided");
                return BadRequest("No data provided for QR code generation");
            }

            // Генерируем QR-код
            using var qrGenerator = new QRCodeGenerator();
            var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            var qrCodeImage = qrCode.GetGraphic(20);

            // Возвращаем QR-код как изображение
            return File(qrCodeImage, "image/png");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error generating QR code");
            return StatusCode(500, "An error occurred while generating the QR code");
        }
    }
} 