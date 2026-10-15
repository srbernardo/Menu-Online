using System.ComponentModel.DataAnnotations;

namespace MenuOnline.Models;

// Equivale ao active_storage_blobs do Rails: o arquivo em si.
// O caminho no disco deriva de Key (sharding), como no ActiveStorage.
public class MediaBlob
{
  public Guid Id { get; set; }
  [Required]
  public string Key { get; set; } = string.Empty;
  [Required]
  public string FileName { get; set; } = string.Empty;
  [Required]
  public string ContentType { get; set; } = string.Empty;
  public long ByteSize { get; set; }
  public string Checksum { get; set; } = string.Empty;
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
  public List<MediaAttachment> Attachments { get; set; } = new List<MediaAttachment>();
}
