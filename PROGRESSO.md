# Menu-Online - Progresso do Projeto

> Atualizado em 2026-10-11. O projeto compila sem erros/avisos (`dotnet build`) e a migration inicial foi gerada a partir do modelo.

## Visão geral

API REST de cardápio online/delivery em **ASP.NET Core (.NET 8)**, **EF Core 8** e **PostgreSQL** (Npgsql), com Swagger em desenvolvimento. Enums são serializados como string no JSON e no banco.

## Regras de negócio modeladas

- **User** tem exatamente **1 Address** e **1 BusinessHours** (FK em `Address.UserId` e `BusinessHours.UserId`, índice único). O User **não pode ser criado sem endereço** (`Address` é obrigatório em `CreateUserRequestDto`). BusinessHours é opcional na criação e pode ser definido depois.
- **Client** tem **vários Address** (`Address.ClientId`) e **vários Orders**.
- **Order** tem **vários Items** via `ItemOrder` (com `Quantity` e `UnitPrice` do momento da compra).
- **Item** tem **1 Category**; **Category** tem **vários Items**.
- **Item** N:N **Extra** via `ItemExtra`.
- Um `Address` pertence a um Client **ou** a um User (`ClientId`/`UserId` nullable).

## Endpoints (todos sob `/api`)

| Recurso | Rotas |
|---|---|
| Item | `GET items?userId=`, `GET item/{id}`, `POST item`, `PUT item/{id}`, `DELETE item/{id}` |
| Category | `GET categories`, `GET category/{id}`, `POST category`, `PUT category/{id}`, `DELETE category/{id}` |
| Extra | `GET extras`, `GET extra/{id}`, `POST extra`, `PUT extra/{id}`, `DELETE extra/{id}` |
| Client | `GET clients`, `GET client/{id}`, `POST client`, `PUT client/{id}`, `DELETE client/{id}` |
| Endereços do cliente | `GET client/{clientId}/addresses`, `GET/PUT/DELETE client/{clientId}/address/{addressId}`, `POST client/{clientId}/address` |
| User | `GET users`, `GET user/{id}`, `POST user`, `PUT user/{id}`, `DELETE user/{id}` (endereço do user é criado/atualizado junto) |
| BusinessHours | `GET/PUT/DELETE user/{userId}/business-hours` (PUT = criar ou atualizar) |
| RateTime | `GET user/{userId}/rate-times`, `POST user/{userId}/rate-time`, `GET/PUT/DELETE rate-time/{id}` |
| Order | `GET orders?userId=&clientId=&status=`, `GET order/{id}`, `POST order`, `PATCH order/{id}/status`, `DELETE order/{id}` |

### Comportamentos relevantes
- **Order**: valida usuário, cliente, endereço de entrega (deve ser do cliente) e itens (devem ser do usuário). Itens repetidos são somados, `UnitPrice` copia o preço atual do item, `TotalAmount = Σ(preço × qtd) + taxa de entrega`, e o `Number` é sequencial por dia/loja (`SetNumberAsync`). Status só avança (Pending → InPreparation → Ready → Delivered); só pedidos `Pending` podem ser cancelados (DELETE).
- **User**: senha com hash (`PasswordHasher`), e-mail único (normalizado em minúsculas).
- **Exclusões protegidas (409 Conflict)**: Category com itens, Client com pedidos, Item já vendido, User com pedidos.
- Controllers retornam sempre DTOs (sem ciclos de serialização) e usam `CreatedAtAction`.

## Correções feitas nesta etapa

- `Address`: propriedades em PascalCase, `ClientId`/`UserId` nullable, `State` obrigatório removido do `[Required]` (enum).
- Removidos `User.AddressId` e `User.BusinessHoursId` (FKs duplicadas que conflitavam nas relações 1:1).
- Cascades perigosos trocados por `Restrict`: Category→Item, Item→ItemOrder, Client→Order, User→Order. `Order.DeliveryAddress` configurado (`SetNull`).
- `Item.Category` sem `[Required]` na navegação.
- `ItemController` reescrito: sem N+1, `Include(ItemExtras → Extra)` (antes usava `Extras` `[NotMapped]`), valida categoria no PUT, atualização de extras por diff, sem `SaveChanges` em loop, `CreatedAtAction`, retorna DTO, `DELETE` retorna 204.
- `ItemMappers.ToItemDto` não consulta mais o banco.
- `UpsetItemRequestDto` renomeado para `UpsertItemRequestDto`.
- `Order.OrderDate` e a numeração diária passam a usar UTC (o Npgsql rejeita `DateTime` com Kind Local em `timestamptz`).
- `ItemOrder` ganhou `Quantity` e `UnitPrice`; `RateTime.Km` agora `decimal(6,2)`.
- `Program.cs`: `JsonStringEnumConverter` e `SuppressAsyncSuffixInActionNames = false`.
- Removidos `WeatherForecast.cs`, `<Folder>` do csproj e `bin/`/`obj/` do índice do git.
- Migration `InitialCreate` gerada em `Migrations/`.

## Ambiente e banco de dados

- PostgreSQL via Docker: `docker compose up -d` (arquivo `docker-compose.yml`, container `menuonline-db`, volume `menuonline_pgdata`).
- Porta **5433** no host, porque a 5432 já é usada por um PostgreSQL local do WSL.
- Connection string: `appsettings.json` não tem mais a senha. A string completa fica em **User Secrets** (`UserSecretsId` no csproj), usado em Development:
  `dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;Port=5433;Database=menuonline;User Id=host;Password=<senha>;"`
- Migration `InitialCreate` **aplicada** (12 tabelas criadas). Para novas: `dotnet ef migrations add <Nome>` e `dotnet ef database update`.
- O compose aceita `DB_PASSWORD` por variável de ambiente (padrão `9090`).

## Fase 1 — Vitrine agregada (concluída em 2026-10-13)

Endpoint único para o front (`menu-online-web/src/services/menuApi.ts`):

- **`GET /api/menu/{slug}`** (`MenuController`): retorna loja + cardápio em uma chamada, no formato exato do contrato do front (`RestaurantMenu`). Slug case-insensitive, `404` se não existir, `categories: []` se a loja não tem itens.
- **`PATCH /api/user/{id}/store-open`** (`{isOpen: bool}`): toggle manual de aberto/fechado da loja.
- Novos campos no `User` (migration `AddStorefrontFields`): `Slug` (único), `Phone`, `CoverUrl`, `LogoUrl`, `IsOpen`. `UserDto`, create e update passam a trafegar esses campos; slug com regex `^[a-z0-9-]+$` e checagem de unicidade (409).
- **`deliveryFee` calculado do `RateTime`**: a vitrine exibe a menor taxa da tabela da loja ("a partir de"); sem tabela, `0` (grátis). O cálculo por distância/endereço no pedido continua pendente.
- **CORS**: política `Frontend` liberando `http://localhost:5173` e `http://127.0.0.1:5173` em Development.
- **Seed** (`Data/DbSeeder.cs`, roda no startup em Development se não houver usuários): loja `burgerfirst` (Burger First, aberta) com endereço, horário de funcionamento, 3 faixas de entrega (R$ 5/8/12), 5 categorias, 3 extras e 14 itens com imagens picsum (mesmas seeds do mock do front). Senha: `loja123456`.

Teste de fumaça (API contra o Postgres do docker): menu 200 com 5 categorias e 14 itens, extras do X-Burger corretos, `deliveryFee` 5, `404` para slug inexistente, toggle off/on refletindo no menu, slug duplicado 409, header `Access-Control-Allow-Origin` presente. Build com 0 erros/avisos.

## Imagens semânticas (2026-10-13)

Troca das imagens aleatórias (picsum) por fotos gastronômicas profissionais (Unsplash, URLs verificadas uma a uma com HTTP 200). Antes houve uma passada com Wikimedia Commons, substituída por não ter qualidade visual adequada: capa (smash + fritas), logo (cheeseburger), 14 itens com foto (antes 4 usavam placeholder). `DbSeeder.cs` e `mockMenu.ts` usam as mesmas URLs; linhas existentes do banco atualizadas via SQL (sem nova migration, só dados).

## Teste de fumaça (original