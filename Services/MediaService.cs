using System.Security.Cryptography;
using MenuOnline.Data;
using MenuOnline.Models;
using Microsoft.EntityFrameworkCore;

namespace MenuOnline.Services;

// Inspiracao no ActiveStorage do Rails: arquivos viram blobs + attachments,
// o upload substitui o anterior (has_one_attached) e a URL vai para a coluna do dono.
public class MediaService
{
  public const long MaxBytes = 5_000_000; // 5MB

  private static readonly Dictionary<string, string> ExtensionsByContentType = new()
  {
    ["image/jpeg"] = ".jpg",
    ["image/png"] = ".png",
    ["image/webp"] = ".webp",
    ["image/gif"] = ".gif"
  };

  private readonly AppDbContext _context;
  private readonly string _uploadRoot;
  private readonly string _publicBaseUrl;

  public MediaService(AppDbContext context, IWebHostEnvironment env, IConfiguration config)
  {
    _context = context;
    _uploadRoot = Path.Combine(env.ContentRootPath, "wwwroot", "uploads");
    _publicBaseUrl = (config["PublicBaseUrl"] ?? "http://localhost:5099").TrimEnd('/');
  }

  public static bool IsValidTriple(string recordType, string name) =>
    (recordType == MediaNames.Item && name == MediaNames.Photo) ||
    (recordType == MediaNames.User && (name == MediaNames.Cover || name == MediaNames.Logo));

  public static string? Validate(IFormFile? file, string recordType, string name)
  {
    if (file == null || file.Length == 0)
      return "Nenhum arquivo enviado.";
    if (file.Length > MaxBytes)
      return "Arquivo muito grande (máximo 5MB).";
    var contentType = (file.ContentType ?? "").ToLowerInvariant();
    if (!ExtensionsByContentType.ContainsKey(contentType))
      return "Apenas imagens são permitidas (jpg, png, webp, gif).";
    if (recordType != MediaNames.Item && recordType != MediaNames.User)
      return "recordType deve ser 'Item' ou 'User'.";
    if (!IsValidTriple(recordType, name))
      return recordType == MediaNames.Item
        ? "name para Item deve ser 'photo'."
        : "name para User deve ser 'cover' ou 'logo'.";
    return null;
  }

  private string UrlFor(MediaBlob blob) =>
    $"{_publicBaseUrl}/uploads/{blob.Key[..2]}/{blob.Key[2..4]}/{blob.Key}{ExtensionsByContentType[blob.ContentType]}";

  // Anexa (substituindo o anterior, se houver) e atualiza a coluna de URL do dono.
  // Retorna o anexo criado. Lanca KeyNotFoundException se o dono nao existir.
  public async Task<MediaAttachment> AttachAsync(string recordType, int recordId, string name, IFormFile file, CancellationToken ct = default)
  {
    object? owner = recordType == MediaNames.Item
      ? await _context.Items.FirstOrDefaultAsync(i => i.Id == recordId, ct)
      : await _context.Users.FirstOrDefaultAsync(u => u.Id == recordId, ct);

    if (owner == null)
      throw new KeyNotFoundException(recordType == MediaNames.Item ? "Item não encontrado." : "Usuário não encontrado.");

    using var memory = new MemoryStream();
    await file.CopyToAsync(memory, ct);
    var bytes = memory.ToArray();
    var contentType = file.ContentType.ToLowerInvariant();
    var key = Guid.NewGuid().ToString("N");
    var relativePath = Path.Combine(key[..2], key[2..4], key + ExtensionsByContentType[contentType]);
    var absolutePath = Path.Combine(_uploadRoot, relativePath);

    Directory.CreateDirectory(Path.GetDirectoryName(absolutePath)!);
    await File.WriteAllBytesAsync(absolutePath, bytes, ct);
    var checksum = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant();

    var old = await _context.MediaAttachments
      .Include(a => a.Blob)
      .FirstOrDefaultAsync(a => a.RecordType == recordType && a.RecordId == recordId && a.Name == name, ct);

    var blob = new MediaBlob
    {
      Id = Guid.NewGuid(),
      Key = key,
      FileName = file.FileName,
      ContentType = contentType,
      ByteSize = bytes.Length,
      Checksum = checksum
    };

    if (old != null)
    {
      DeleteFile(old.Blob);
      _context.MediaAttachments.Remove(old);
      if (old.Blob != null)
        _context.MediaBlobs.Remove(old.Blob);
    }

    var attachment = new MediaAttachment
    {
      RecordType = recordType,
      RecordId = recordId,
      Name = name,
      Blob = blob
    };
    await _context.MediaAttachments.AddAsync(attachment, ct);

    var url = UrlFor(blob);
    ApplyOwnerUrl(owner, name, url);

    await _context.SaveChangesAsync(ct);
    return attachment;
  }

  // Remove o anexo (arquivo + linhas) e limpa a coluna de URL do dono.
  public async Task<bool> PurgeAsync(string recordType, int recordId, string name, CancellationToken ct = default)
  {
    var attachment = await _context.MediaAttachments
      .Include(a => a.Blob)
      .FirstOrDefaultAsync(a => a.RecordType == recordType && a.RecordId == recordId && a.Name == name, ct);

    if (attachment == null)
      return false;

    DeleteFile(attachment.Blob);
    _context.MediaAttachments.Remove(attachment);
    if (attachment.Blob != null)
      _context.MediaBlobs.Remove(attachment.Blob);

    object? owner = recordType == MediaNames.Item
      ? await _context.Items.FirstOrDefaultAsync(i => i.Id == recordId, ct)
      : await _context.Users.FirstOrDefaultAsync(u => u.Id == recordId, ct);
    if (owner != null)
      ApplyOwnerUrl(owner, name, "");

    await _context.SaveChangesAsync(ct);
    return true;
  }

  // Remove todos os anexos do dono (usado ao excluir item/usuario).
  public async Task PurgeOwnerAsync(string recordType, int recordId, CancellationToken ct = default)
  {
    var attachments = await _context.MediaAttachments
      .Include(a => a.Blob)
      .Where(a => a.RecordType == recordType && a.RecordId == recordId)
      .ToListAsync(ct);

    foreach (var attachment in attachments)
    {
      DeleteFile(attachment.Blob);
      _context.MediaAttachments.Remove(attachment);
      if (attachment.Blob != null)
        _context.MediaBlobs.Remove(attachment.Blob);
    }

    if (attachments.Count > 0)
      await _context.SaveChangesAsync(ct);
  }

  public Task<MediaBlob?> GetBlobAsync(Guid blobId, CancellationToken ct = default) =>
    _context.MediaBlobs.AsNoTracking().FirstOrDefaultAsync(b => b.Id == blobId, ct);

  public string PublicUrl(MediaBlob blob) => UrlFor(blob);

  private static void ApplyOwnerUrl(object owner, string name, string url)
  {
    switch (owner)
    {
      case Item item: item.ImageUrl = url; break;
      case User user when name == MediaNames.Cover: user.CoverUrl = url; break;
      case User user when name == MediaNames.Logo: user.LogoUrl = url; break;
    }
  }

  private void DeleteFile(MediaBlob? blob)
  {
    if (blob == null || !ExtensionsByContentType.TryGetValue(blob.ContentType, out var ext))
      return;
    var path = Path.Combine(_uploadRoot, blob.Key[..2], blob.Key[2..4], blob.Key + ext);
    try { if (File.Exists(path)) File.Delete(path); } catch { /* melhor esforco */ }
  }
}
