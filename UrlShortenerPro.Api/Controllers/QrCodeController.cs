using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Security.Claims;
using System.Threading.Tasks;
using UrlShortenerPro.Core.Interfaces;

namespace UrlShortenerPro.Api.Controllers;

[ApiController]
[Route("api/qr-code")]
public class QrCodeController : ControllerBase
{
    private readonly ILogger<QrCodeController> _logger;
    private readonly IUrlRepository _urlRepository;

    public QrCodeController(
        ILogger<QrCodeController> logger,
        IUrlRepository urlRepository)
    {
        _logger = logger;
        _urlRepository = urlRepository;
    }

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
                _logger.LogWarning("QR code generation failed: No data provided");
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
            _logger.LogError(ex, "Error generating QR code");
            return StatusCode(500, "An error occurred while generating the QR code");
        }
    }

    /// <summary>
    /// Отмечает ссылку как имеющую QR-код
    /// </summary>
    /// <param name="shortCode">Короткий код ссылки</param>
    /// <returns>Результат операции</returns>
    [HttpPost]
    [Route("mark-as-generated")]
    [Authorize]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> MarkAsGenerated([FromQuery] string shortCode)
    {
        try
        {
            // Получаем ID пользователя из токена
            if (!int.TryParse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            {
                _logger.LogWarning("User ID claim not found or invalid in token");
                return Unauthorized();
            }

            // Находим ссылку по короткому коду
            var url = await _urlRepository.GetByShortCodeAsync(shortCode);
            if (url == null)
            {
                return NotFound($"URL with short code '{shortCode}' not found");
            }

            // Проверяем, принадлежит ли ссылка пользователю
            if (url.UserId != userId)
            {
                return Unauthorized("You don't have permission to modify this URL");
            }

            // Отмечаем, что для ссылки сгенерирован QR-код
            url.HasQrCode = true;
            await _urlRepository.UpdateAsync(url);

            return Ok(new { message = "URL marked as having QR code" });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error marking URL as having QR code");
            return StatusCode(500, "An error occurred while processing your request");
        }
    }
} 