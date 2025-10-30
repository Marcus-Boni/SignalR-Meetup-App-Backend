# ?? SignalR Meetup App - Backend

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-00C7B7)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Backend moderno em **ASP.NET Core** com **SignalR** para demonstra��o de comunica��o em tempo real, incluindo rastreamento de ve�culos, chat multi-sala e notifica��es de pagamento.

---

## ?? �ndice

- [Sobre o Projeto](#-sobre-o-projeto)
- [Funcionalidades](#-funcionalidades)
- [Tecnologias](#?-tecnologias)
- [Arquitetura](#-arquitetura)
- [Pr�-requisitos](#-pr�-requisitos)
- [Instala��o](#-instala��o)
- [Configura��o](#?-configura��o)
- [Uso](#-uso)
- [Endpoints da API](#-endpoints-da-api)
- [Hubs do SignalR](#-hubs-do-signalr)
- [Autentica��o](#-autentica��o)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Deploy](#-deploy)
- [Contribui��o](#-contribui��o)
- [Licen�a](#-licen�a)

---

## ?? Sobre o Projeto

Este projeto foi desenvolvido como parte de um **Meetup sobre SignalR** e demonstra implementa��es pr�ticas de comunica��o em tempo real usando ASP.NET Core. O backend oferece tr�s casos de uso reais:

1. **?? Rastreamento de Ve�culos**: Simula��o de GPS em tempo real com movimenta��o suave
2. **?? Chat Multi-Sala**: Sistema de chat com m�ltiplas salas e gerenciamento de grupos
3. **?? Notifica��es de Pagamento**: Processamento ass�ncrono com atualiza��es em tempo real

O projeto segue princ�pios de **Clean Architecture** e **SOLID**, separando responsabilidades entre Controllers, Services e Hubs.

---

## ? Funcionalidades

### ?? Autentica��o JWT
- Login com usu�rios de demonstra��o
- Tokens JWT para autentica��o segura
- Valida��o de tokens em tempo real
- Suporte a autentica��o via query string (SignalR)

### ?? Rastreamento de Ve�culos (TrackingHub)
- Simula��o realista de movimento de ve�culo
- Interpola��o suave entre waypoints
- Dados detalhados: velocidade, dire��o, status
- Paradas em pontos de entrega
- Broadcast server-to-client autom�tico

### ?? Chat em Tempo Real (ChatHub)
- M�ltiplas salas de chat
- Join/Leave de salas dinamicamente
- Notifica��es de entrada/sa�da de usu�rios
- Mensagens com timestamp e identifica��o de usu�rio
- Suporte a autentica��o obrigat�ria

### ?? Processamento de Pagamentos (PaymentHub)
- Simula��o de processamento ass�ncrono
- Notifica��es de status em tempo real
- Grupos privados por usu�rio e pedido
- Estados: Pending ? Processing ? Completed/Failed
- Fire-and-forget pattern

---

## ??? Tecnologias

- **[.NET 9.0](https://dotnet.microsoft.com/)** - Framework principal
- **[ASP.NET Core SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr)** - Comunica��o em tempo real
- **[JWT Bearer Authentication](https://jwt.io/)** - Autentica��o e autoriza��o
- **[Swagger/OpenAPI](https://swagger.io/)** - Documenta��o da API
- **[Hosted Services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)** - Background tasks

### Pacotes NuGet
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.10" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.9" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="9.0.6" />
```

---

## ??? Arquitetura

### Padr�es Implementados

- **Clean Architecture**: Separa��o clara entre camadas
- **Dependency Injection**: Inje��o de depend�ncias nativa do ASP.NET Core
- **Service Layer**: L�gica de neg�cio isolada em services
- **DTOs (Data Transfer Objects)**: Contratos de comunica��o entre camadas
- **Background Services**: Processamento ass�ncrono com `IHostedService`
- **Hub Pattern**: Thin Hubs focados apenas em comunica��o

### Fluxo de Dados

```
Client (Next.js/React)
    ?
???????????????????????????????????????
?   Controllers (HTTP REST)           ?
?   - AuthController                  ?
?   - PaymentsController              ?
???????????????????????????????????????
    ?
???????????????????????????????????????
?   Services (Business Logic)         ?
?   - IAuthService                    ?
?   - IPaymentService                 ?
?   - CarTrackingService              ?
???????????????????????????????????????
    ?
???????????????????????????????????????
?   SignalR Hubs (Real-time)          ?
?   - TrackingHub                     ?
?   - ChatHub                         ?
?   - PaymentHub                      ?
???????????????????????????????????????
    ?
Client (WebSocket Connection)
```

---

## ?? Pr�-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) ou superior
- IDE recomendada:
  - [Visual Studio 2022](https://visualstudio.microsoft.com/) (17.8+)
  - [Visual Studio Code](https://code.visualstudio.com/) com extens�o C#
  - [JetBrains Rider](https://www.jetbrains.com/rider/)
- [Git](https://git-scm.com/)

---

## ?? Instala��o

### 1. Clone o reposit�rio

```bash
git clone https://github.com/Marcus-Boni/SignalR-Meetup-App-Backend.git
cd SignalR-Meetup-App-Backend
```

### 2. Restaure as depend�ncias

```bash
dotnet restore
```

### 3. Execute o projeto

```bash
dotnet run
```

O servidor estar� dispon�vel em:
- **HTTPS**: `https://localhost:5001`
- **HTTP**: `http://localhost:5000`

---

## ?? Configura��o

### appsettings.json

Configure as seguintes se��es no arquivo `appsettings.json`:

```json
{
  "Jwt": {
    "Key": "SuaChaveSecretaSuperSegura_MinimoDe32Caracteres!",
    "Issuer": "CarTrackingApi",
    "Audience": "CarTrackingApiClients"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.AspNetCore.SignalR": "Debug"
    }
  }
}
```

### CORS

Para desenvolvimento local com frontend, ajuste a pol�tica CORS em `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",  // Next.js local
                "https://seu-app.vercel.app"  // Produ��o
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

---

## ?? Uso

### 1. Autentica��o

Primeiro, obtenha um token JWT:

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

**Usu�rios de demonstra��o:**
- `admin` / `admin123`
- `usuario1` / `senha123`
- `usuario2` / `senha456`
- `teste` / `teste123`

**Resposta:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "username": "admin",
  "expiresAt": "2024-01-20T15:30:00Z"
}
```

### 2. Conectar ao SignalR (JavaScript)

```javascript
import * as signalR from "@microsoft/signalr";

const token = "seu-token-jwt";

// Conex�o com autentica��o
const connection = new signalR.HubConnectionBuilder()
  .withUrl("https://localhost:5001/chatHub", {
    accessTokenFactory: () => token
  })
  .withAutomaticReconnect()
  .build();

// Conectar
await connection.start();
console.log("? Conectado ao SignalR");

// Entrar em uma sala
await connection.invoke("JoinRoom", "sala-geral");

// Receber mensagens
connection.on("ReceiveMessage", (message) => {
  console.log(`${message.user}: ${message.message}`);
});

// Enviar mensagem
await connection.invoke("SendMessage", "sala-geral", "Ol�, pessoal!");
```

---

## ?? Endpoints da API

### Autentica��o

#### POST `/api/auth/login`
Autentica um usu�rio e retorna um token JWT.

**Request Body:**
```json
{
  "username": "admin",
  "password": "admin123"
}
```

**Response:**
```json
{
  "token": "eyJhbGc...",
  "username": "admin",
  "expiresAt": "2024-01-20T15:30:00Z"
}
```

#### GET `/api/auth/validate`
Valida um token JWT (requer autentica��o).

**Response:**
```json
{
  "isValid": true,
  "username": "admin",
  "userId": "1",
  "claims": [...]
}
```

#### GET `/api/auth/health`
Health check do servi�o de autentica��o.

---

### Pagamentos

#### POST `/api/payments/pay`
Inicia um pagamento ass�ncrono (requer autentica��o).

**Request Body:**
```json
{
  "orderId": "ORD-12345",
  "amount": 150.50,
  "description": "Compra de produtos"
}
```

**Response (202 Accepted):**
```json
{
  "orderId": "ORD-12345",
  "status": "Processing initiated",
  "message": "Seu pagamento foi recebido...",
  "timestamp": "2024-01-20T14:25:00Z"
}
```

> **Nota:** O status do pagamento ser� enviado via SignalR (PaymentHub).

#### GET `/api/payments/health`
Health check do servi�o de pagamentos.

---

## ?? Hubs do SignalR

### TrackingHub (`/trackingHub`)

**Server-to-Client (Broadcast autom�tico)**

| M�todo | Par�metros | Descri��o |
|--------|-----------|-----------|
| `ReceivePosition` | `VehiclePosition` | Recebe posi��o atualizada do ve�culo a cada 50ms |

**VehiclePosition:**
```typescript
{
  x: number;
  y: number;
  speed: number;
  heading: number;
  status: "Moving" | "Stopped" | "Accelerating" | "Braking";
  timestamp: string;
  routeProgress: number; // 0-100
}
```

---

### ChatHub (`/chatHub`)

**Client-to-Server**

| M�todo | Par�metros | Retorno | Descri��o |
|--------|-----------|---------|-----------|
| `JoinRoom` | `roomName: string` | `void` | Entra em uma sala de chat |
| `LeaveRoom` | `roomName: string` | `void` | Sai de uma sala de chat |
| `SendMessage` | `roomName: string, message: string` | `void` | Envia mensagem para a sala |

**Server-to-Client**

| M�todo | Par�metros | Descri��o |
|--------|-----------|-----------|
| `ReceiveMessage` | `ChatMessageDto` | Recebe nova mensagem na sala |
| `UserJoined` | `{user, roomName, timestamp}` | Notifica entrada de usu�rio |
| `UserLeft` | `{user, roomName, timestamp}` | Notifica sa�da de usu�rio |

**ChatMessageDto:**
```typescript
{
  user: string;
  message: string;
  roomName: string;
  timestamp: string;
}
```

---

### PaymentHub (`/paymentHub`)

**Client-to-Server**

| M�todo | Par�metros | Retorno | Descri��o |
|--------|-----------|---------|-----------|
| `SubscribeToPaymentStatus` | `orderId: string` | `void` | Inscreve-se para atualiza��es de pagamento |
| `UnsubscribeFromPaymentStatus` | `orderId: string` | `void` | Cancela inscri��o |

**Server-to-Client**

| M�todo | Par�metros | Descri��o |
|--------|-----------|-----------|
| `SubscriptionConfirmed` | `{orderId, message, timestamp}` | Confirma inscri��o |
| `PaymentStatusUpdated` | `PaymentStatusDto` | Atualiza��o de status do pagamento |

**PaymentStatusDto:**
```typescript
{
  orderId: string;
  status: "Pending" | "Processing" | "Completed" | "Failed";
  message: string;
  timestamp: string;
}
```

---

## ?? Autentica��o

### Fluxo de Autentica��o JWT

1. Cliente faz login via `/api/auth/login`
2. Backend valida credenciais e retorna token JWT
3. Cliente armazena o token (localStorage/sessionStorage)
4. Para HTTP requests: adiciona header `Authorization: Bearer {token}`
5. Para SignalR: passa token via `accessTokenFactory` ou query string

### Autentica��o em SignalR

O projeto suporta dois m�todos:

**1. Via Token Factory (Recomendado):**
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl("/chatHub", {
    accessTokenFactory: () => getStoredToken()
  })
  .build();
```

**2. Via Query String:**
```javascript
const connection = new signalR.HubConnectionBuilder()
  .withUrl(`/chatHub?access_token=${token}`)
  .build();
```

---

## ?? Estrutura do Projeto

```
Meetup-WebSocket/
??? Controllers/
?   ??? AuthController.cs          # Endpoints de autentica��o
?   ??? PaymentsController.cs      # Endpoints de pagamento
??? Hubs/
?   ??? TrackingHub.cs            # Hub de rastreamento
?   ??? ChatHub.cs                # Hub de chat
?   ??? PaymentHub.cs             # Hub de pagamentos
??? Services/
?   ??? IAuthService.cs           # Interface de autentica��o
?   ??? AuthService.cs            # Implementa��o de autentica��o
?   ??? IPaymentService.cs        # Interface de pagamento
?   ??? PaymentService.cs         # L�gica de pagamento
?   ??? CarTrackingService.cs     # Background service de rastreamento
??? Dtos/
?   ??? LoginRequestDto.cs        # DTO de login
?   ??? LoginResponseDto.cs       # DTO de resposta de login
?   ??? ChatMessageDto.cs         # DTO de mensagem de chat
?   ??? PaymentRequestDto.cs      # DTO de requisi��o de pagamento
?   ??? PaymentStatusDto.cs       # DTO de status de pagamento
??? Properties/
?   ??? launchSettings.json       # Configura��es de execu��o
??? appsettings.json              # Configura��es da aplica��o
??? appsettings.Development.json  # Configura��es de desenvolvimento
??? Program.cs                    # Ponto de entrada e configura��o
??? Meetup-WebSocket.csproj       # Arquivo de projeto
```

---

## ?? Deploy

### Docker (Recomendado)

Crie um `Dockerfile`:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src
COPY ["Meetup-WebSocket.csproj", "./"]
RUN dotnet restore
COPY . .
RUN dotnet build -c Release -o /app/build

FROM build AS publish
RUN dotnet publish -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Meetup-WebSocket.dll"]
```

**Build e Run:**
```bash
docker build -t signalr-backend .
docker run -p 5000:80 signalr-backend
```

### Azure App Service

1. Publique via Visual Studio (bot�o direito no projeto ? Publish)
2. Ou via CLI:
```bash
az webapp up --name signalr-meetup-api --resource-group MeuGrupo
```

### Vari�veis de Ambiente (Produ��o)

Configure as seguintes vari�veis de ambiente:

```bash
Jwt__Key=SuaChaveSecretaSegura
Jwt__Issuer=SeuIssuer
Jwt__Audience=SeuAudience
ASPNETCORE_ENVIRONMENT=Production
```

---

## ?? Contribui��o

Contribui��es s�o bem-vindas! Siga os passos:

1. Fa�a um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/NovaFuncionalidade`)
3. Commit suas mudan�as (`git commit -m 'Adiciona nova funcionalidade'`)
4. Push para a branch (`git push origin feature/NovaFuncionalidade`)
5. Abra um Pull Request

### Padr�es de C�digo

- Siga as conven��es do C# (.NET)
- Use `async/await` para opera��es ass�ncronas
- Adicione XML documentation comments em m�todos p�blicos
- Escreva logs informativos para debugging
- Mantenha os Hubs "thin" (l�gica de neg�cio nos Services)

---

## ?? Licen�a

Este projeto est� sob a licen�a MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

## ????? Autor

**Marcus Vinicius Galv�o Boni**

- GitHub: [@Marcus-Boni](https://github.com/Marcus-Boni)
- Repository: [SignalR-Meetup-App-Backend](https://github.com/Marcus-Boni/SignalR-Meetup-App-Backend)

---

## ?? Agradecimentos

- Comunidade .NET
- ASP.NET Core Team
- SignalR Contributors
- Participantes do Meetup

---

## ?? Recursos Adicionais

- [Documenta��o oficial do SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/)
- [JWT Authentication no ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [Background Services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
- [CORS no ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/cors)

---

<div align="center">

**? Se este projeto foi �til, considere dar uma estrela!**

Made with ?? and .NET

</div>
