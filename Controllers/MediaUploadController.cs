using MenuOnline.Dtos.Media;
using MenuOnline.Services;
using Microsoft.AspNetCore.Mvc;

namespace MenuOnline.Controllers
{
  // Upload generico estilo ActiveStorage: um endpoint para foto do item,
  // capa e logo da loja. A URL resultante vai para a coluna do dono,
  // entao todas as leituras existentes passam a exibi-la sem mudanca.
  //
  //   POST /api/upload (multipart: file, recordType=Item|User, recordId, name=photo|cover|logo)
  //   GET  /api/upload/{blobId}
  //   DELETE /api/upload/{recordType}/{recordId}/{name}
  [Route("api")]
  [ApiController]
  public class MediaUploadController : ControllerBase
  {
    private readonly MediaService _media;
    private readonly ILogger<MediaUploadController> _logger;

    public MediaUploadController(MediaService media, ILogger<MediaUploadController> logger)
    {
      _media = media;
      _logger = logger;
    }

    [HttpPost("upload")]
    public async Task<IActionResult> UploadAsync(
      [FromForm] IFormFile? file,
      [FromForm] string recordType = "",
      [FromForm] int recordId = 0,
      [FromForm] string name = "",
      CancellationToken ct = default)
    {
      var error = MediaService.Validate(file, recordType, name);
      if (error != null)
        return BadRequest(error);

      try
      {
        var attachment = await _media.AttachAsync(recordType, recordId, name, file!, ct);
        var blob = attachment.Blob!;
        return StatusCode(201, new UploadResponseDto(blob.Id, attachment.Id, _media.PublicUrl(blob),
          blob.FileName, blob.ContentType, blob.ByteSize));
      }
      catch (KeyNotFoundException e)
      {
        return NotFound(e.Message);
      }
      catch (Exception e)
      {
        _logger.LogError(e, "Erro ao fazer upload");
        return StatusCode(500);
      }
    }

    [HttpGet("upload/{blobId:guid}")]
    public async Task<IActionResult> GetByIdAsync([FromRoute] Guid blobId, CancellationToken ct = default)
    {
      var blob = await _media.GetBlobAsync(blobId, ct);
      if (blob == null)
        return NotFound();

      return Ok(new UploadResponseDto(blob.Id, 0, _media.PublicUrl(blob),
        blob.FileName, blob.ContentType, blob.ByteSize));
    }

    [HttpDelete("upload/{recordType}/{recordId:int}/{name}")]
    public async Task<IActionResult> PurgeAsync(
      [FromRoute] string recordType, [FromRoute] int recordId, [FromRoute] string name,
      CancellationToken ct = default)
    {
      if (!MediaService.IsValidTriple(recordType, name))
        return BadRequest("Combinação recordType/name inválida.");

      var removed = await _media.PurgeAsync(recordType, recordId, name, ct);
      if (!removed)
        return NotFound();

      return NoContent();
    }
  }
}
