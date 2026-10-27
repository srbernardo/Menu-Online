using System.ComponentModel.DataAnnotations;

namespace MenuOnline.Models;

// Equivale ao active_storage_attachments: liga um blob a um dono.
// Name funciona como o `has_one_attached :photo` (photo, cover, logo).
public class MediaAttachment
{
  public int Id { get; set; }
  [Required]
  public string Name { get; set; } = string.Empty;
  [Required]
  public string RecordType { get; set; } = string.Empty;
  public int RecordId { get; set; }
  public Guid BlobId { get; set; }
  public MediaBlob? Blob { get; set; }
  public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public static class MediaNames
{
  public const string Item = "Item";
  public const string User = "User";
  public const string Photo = "photo";
  public const string Cover = "cover";
  public const string Logo = "logo";
}
