using System.ComponentModel.DataAnnotations;

namespace MenuOnline.Dtos.RateTime;

public record UpsertRateTimeRequestDto
(
  [Required] string TimeInMinutes,
  [Range(0, 9999)] decimal Km,
  [Range(0, 100000)] decimal Rate
);
