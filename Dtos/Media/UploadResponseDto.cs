namespace MenuOnline.Dtos.Media;

public record UploadResponseDto
(
  Guid BlobId,
  int AttachmentId,
  string Url,
  string Filename,
  string ContentType,
  long ByteSize
);
