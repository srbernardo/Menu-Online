namespace MenuOnline.Dtos.Client;

// Resposta do check feito enquanto o cliente digita o telefone.
// Nao expoe o cadastro completo: apenas o nome para autocompletar o form.
public record ClientLookupResponseDto
(
  bool Exists,
  string? Name
);
