using Microsoft.AspNetCore.Mvc;
using ServerlessKakeibo.Api.Service.Interface;
using ServerlessKakeibo.Api.Service.Models;
using Swashbuckle.AspNetCore.Annotations;

namespace ServerlessKakeibo.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TestController : ControllerBase
{
    private readonly IVertexAiService _vertexAiService;

    public TestController(IVertexAiService vertexAiService)
    {
        _vertexAiService = vertexAiService;
    }

    [HttpGet("vertex")]
    public async Task<IActionResult> TestVertex()
    {
        var result = await _vertexAiService.GenerateContentAsync("テストです");
        return Ok(new { result });
    }

    /// <summary>
    /// 画像付きテスト（ファイルアップロード版）
    /// </summary>
    [HttpPost("vertex-with-image")]
    [Consumes("multipart/form-data")]
    [SwaggerOperation(Summary = "画像付きでVertex AIをテスト")]
    public async Task<IActionResult> TestVertexWithImage(
        [FromForm] VertexImageForm form,
        [FromForm] string? systemPrompt = null)
    {
        try
        {
            List<ImageAttachment>? images = null;

            if (form.Image != null)
            {
                // 画像をBase64に変換
                using var memoryStream = new MemoryStream();
                await form.Image.CopyToAsync(memoryStream);
                var imageBase64 = Convert.ToBase64String(memoryStream.ToArray());

                images = new List<ImageAttachment>
                {
                    new ImageAttachment
                    {
                        Base64Data = imageBase64,
                        MimeType = form.Image.ContentType ?? "image/jpeg"
                    }
                };
            }

            var result = await _vertexAiService.GenerateContentAsync(
                userPrompt: form.Prompt,
                systemPrompt: systemPrompt,
                images: images
            );

            return Ok(new
            {
                success = true,
                result,
                imageInfo = form.Image != null ? new
                {
                    fileName = form.Image.FileName,
                    size = form.Image.Length,
                    contentType = form.Image.ContentType
                } : null
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }

    /// <summary>
    /// 画像付きテスト（Base64版）
    /// </summary>
    [HttpPost("vertex-with-base64")]
    public async Task<IActionResult> TestVertexWithBase64([FromBody] TestImageRequest request)
    {
        try
        {
            List<ImageAttachment>? images = null;

            if (!string.IsNullOrEmpty(request.ImageBase64))
            {
                images = new List<ImageAttachment>
                {
                    new ImageAttachment
                    {
                        Base64Data = request.ImageBase64,
                        MimeType = request.MimeType ?? "image/jpeg"
                    }
                };
            }

            var result = await _vertexAiService.GenerateContentAsync(
                userPrompt: request.Prompt ?? "この画像について説明してください",
                systemPrompt: request.SystemPrompt,
                images: images
            );

            return Ok(new { success = true, result });
        }
        catch (Exception ex)
        {
            return BadRequest(new { success = false, error = ex.Message });
        }
    }
}

// リクエストDTO
public class TestImageRequest
{
    public string? Prompt { get; set; }
    public string? SystemPrompt { get; set; }
    public string? ImageBase64 { get; set; }
    public string? MimeType { get; set; }
}

public class VertexImageForm
{
    public IFormFile Image { get; set; } = default!;
    public string Prompt { get; set; } = "この画像について説明してください";
}
