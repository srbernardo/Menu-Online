namespace MenuOnline.Services;

// Login por telefone: aceita numero com ou sem mascara e compara so os digitos.
public static class PhoneNumber
{
  public static string Normalize(string? phone)
  {
    return new string((phone ?? "").Where(char.IsDigit).ToArray());
  }

  // DDD (2) + numero (8 ou 9).
  public static bool IsValid(string normalized)
  {
    return normalized.Length is 10 or 11;
  }
}
