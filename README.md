# 🚀 SignalR Meetup App - Backend

[![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![SignalR](https://img.shields.io/badge/SignalR-Real--time-00C7B7)](https://dotnet.microsoft.com/apps/aspnet/signalr)
[![License](https://img.shields.io/badge/license-MIT-blue.svg)](LICENSE)

Backend moderno em **ASP.NET Core** com **SignalR** para demonstração de comunicação em tempo real, incluindo rastreamento de veículos, chat multi-sala e notificações de pagamento.

---

## 📋 Índice

- [Sobre o Projeto](#-sobre-o-projeto)
- [Funcionalidades](#-funcionalidades)
- [Tecnologias](#-tecnologias)
- [Arquitetura](#-arquitetura)
- [Pré-requisitos](#-pré-requisitos)
- [Instalação](#-instalação)
- [Configuração](#-configuração)
- [Uso](#-uso)
- [Endpoints da API](#-endpoints-da-api)
- [Hubs do SignalR](#-hubs-do-signalr)
- [Autenticação](#-autenticação)
- [Estrutura do Projeto](#-estrutura-do-projeto)
- [Deploy](#-deploy)
- [Contribuição](#-contribuição)
- [Licença](#-licença)

---

## 🎯 Sobre o Projeto

Este projeto foi desenvolvido como parte de um **Meetup sobre SignalR** e demonstra implementações práticas de comunicação em tempo real usando ASP.NET Core. O backend oferece três casos de uso reais:

1. **🚗 Rastreamento de Veículos**: Simulação de GPS em tempo real com movimentação suave
2. **💬 Chat Multi-Sala**: Sistema de chat com múltiplas salas e gerenciamento de grupos
3. **💳 Notificações de Pagamento**: Processamento assíncrono com atualizações em tempo real

O projeto segue princípios de **Clean Architecture** e **SOLID**, separando responsabilidades entre Controllers, Services e Hubs.

---

## ✨ Funcionalidades

### 🔐 Autenticação JWT
- Login com usuários de demonstração
- Tokens JWT para autenticação segura
- Validação de tokens em tempo real
- Suporte a autenticação via query string (SignalR)

### 🚗 Rastreamento de Veículos (TrackingHub)
- Simulação realista de movimento de veículo
- Interpolação suave entre waypoints
- Dados detalhados: velocidade, direção, status
- Paradas em pontos de entrega
- Broadcast server-to-client automático

### 💬 Chat em Tempo Real (ChatHub)
- Múltiplas salas de chat
- Join/Leave de salas dinamicamente
- Notificações de entrada/saída de usuários
- Mensagens com timestamp e identificação de usuário
- Suporte a autenticação obrigatória

### 💳 Processamento de Pagamentos (PaymentHub)
- Simulação de processamento assíncrono
- Notificações de status em tempo real
- Grupos privados por usuário e pedido
- Estados: Pending ? Processing ? Completed/Failed
- Fire-and-forget pattern

---

## 🛠️ Tecnologias

- **[.NET 9.0](https://dotnet.microsoft.com/)** - Framework principal
- **[ASP.NET Core SignalR](https://dotnet.microsoft.com/apps/aspnet/signalr)** - Comunicação em tempo real
- **[JWT Bearer Authentication](https://jwt.io/)** - Autenticação e autorização
- **[Swagger/OpenAPI](https://swagger.io/)** - Documentação da API
- **[Hosted Services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)** - Background tasks

### Pacotes NuGet
```xml
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="9.0.10" />
<PackageReference Include="Microsoft.AspNetCore.OpenApi" Version="9.0.9" />
<PackageReference Include="Swashbuckle.AspNetCore" Version="9.0.6" />
```

---

## 🏗️ Arquitetura

### Padrões Implementados

- **Clean Architecture**: Separação clara entre camadas
- **Dependency Injection**: Injeção de dependências nativa do ASP.NET Core
- **Service Layer**: Lógica de negócio isolada em services
- **DTOs (Data Transfer Objects)**: Contratos de comunicação entre camadas
- **Background Services**: Processamento assíncrono com `IHostedService`
- **Hub Pattern**: Thin Hubs focados apenas em comunicação

### Fluxo de Dados

```
Client (Next.js/React)
    ↓
┌─────────────────────────────────────┐
│   Controllers (HTTP REST)           │
│   - AuthController                  │
│   - PaymentsController              │
└─────────────────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│   Services (Business Logic)         │
│   - IAuthService                    │
│   - IPaymentService                 │
│   - CarTrackingService              │
└─────────────────────────────────────┘
    ↓
┌─────────────────────────────────────┐
│   SignalR Hubs (Real-time)          │
│   - TrackingHub                     │
│   - ChatHub                         │
│   - PaymentHub                      │
└─────────────────────────────────────┘
    ↓
Client (WebSocket Connection)
```

---

## 📦 Pré-requisitos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0) ou superior
- IDE recomendada:
  - [Visual Studio 2022](https://visualstudio.microsoft.com/) (17.8+)
  - [Visual Studio Code](https://code.visualstudio.com/) com extensão C#
  - [JetBrains Rider](https://www.jetbrains.com/rider/)
- [Git](https://git-scm.com/)

---

## 🚀 Instalação

### 1. Clone o repositório

```bash
git clone https://github.com/Marcus-Boni/SignalR-Meetup-App-Backend.git
cd SignalR-Meetup-App-Backend
```

### 2. Restaure as dependências

```bash
dotnet restore
```

### 3. Execute o projeto

```bash
dotnet run
```

O servidor estará disponível em:
- **HTTPS**: `https://localhost:5001`
- **HTTP**: `http://localhost:5000`

---

## ⚙️ Configuração

### CORS

Para desenvolvimento local com frontend, ajuste a política CORS em `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextApp", policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",  // Next.js local
                "https://seu-app.vercel.app"  // Produção
              )
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});
```

---

## 💻 Uso

### 1. Autenticação

Primeiro, obtenha um token JWT:

```bash
curl -X POST https://localhost:5001/api/auth/login \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin123"
  }'
```

**Usuários de demonstração:**
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

// Conexão com autenticação
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
await connection.invoke("SendMessage", "sala-geral", "Olá, pessoal!");
```

---

## 📡 Endpoints da API

### Autenticação

#### POST `/api/auth/login`
Autentica um usuário e retorna um token JWT.

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
Valida um token JWT (requer autenticação).

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
Health check do serviço de autenticação.

---

### Pagamentos

#### POST `/api/payments/pay`
Inicia um pagamento assíncrono (requer autenticação).

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

> **Nota:** O status do pagamento será enviado via SignalR (PaymentHub).

#### GET `/api/payments/health`
Health check do serviço de pagamentos.

---

## 🔌 Hubs do SignalR

### TrackingHub (`/trackingHub`)

**Server-to-Client (Broadcast automático)**

| Método | Parâmetros | Descrição |
|--------|-----------|-----------|
| `ReceivePosition` | `VehiclePosition` | Recebe posição atualizada do veículo a cada 50ms |

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

| Método | Parâmetros | Retorno | Descrição |
|--------|-----------|---------|-----------|
| `JoinRoom` | `roomName: string` | `void` | Entra em uma sala de chat |
| `LeaveRoom` | `roomName: string` | `void` | Sai de uma sala de chat |
| `SendMessage` | `roomName: string, message: string` | `void` | Envia mensagem para a sala |

**Server-to-Client**

| Método | Parâmetros | Descrição |
|--------|-----------|-----------|
| `ReceiveMessage` | `ChatMessageDto` | Recebe nova mensagem na sala |
| `UserJoined` | `{user, roomName, timestamp}` | Notifica entrada de usuário |
| `UserLeft` | `{user, roomName, timestamp}` | Notifica saída de usuário |

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

| Método | Parâmetros | Retorno | Descrição |
|--------|-----------|---------|-----------|
| `SubscribeToPaymentStatus` | `orderId: string` | `void` | Inscreve-se para atualizações de pagamento |
| `UnsubscribeFromPaymentStatus` | `orderId: string` | `void` | Cancela inscrição |

**Server-to-Client**

| Método | Parâmetros | Descrição |
|--------|-----------|-----------|
| `SubscriptionConfirmed` | `{orderId, message, timestamp}` | Confirma inscrição |
| `PaymentStatusUpdated` | `PaymentStatusDto` | Atualização de status do pagamento |

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

## 🔐 Autenticação

### Fluxo de Autenticação JWT

1. Cliente faz login via `/api/auth/login`
2. Backend valida credenciais e retorna token JWT
3. Cliente armazena o token (localStorage/sessionStorage)
4. Para HTTP requests: adiciona header `Authorization: Bearer {token}`
5. Para SignalR: passa token via `accessTokenFactory` ou query string

### Autenticação em SignalR

O projeto suporta dois métodos:

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

## 📁 Estrutura do Projeto

```
Meetup-WebSocket/
├── Controllers/
│   ├── AuthController.cs          # Endpoints de autenticação
│   └── PaymentsController.cs      # Endpoints de pagamento
├── Hubs/
│   ├── TrackingHub.cs            # Hub de rastreamento
│   ├── ChatHub.cs                # Hub de chat
│   └── PaymentHub.cs             # Hub de pagamentos
├── Services/
│   ├── IAuthService.cs           # Interface de autenticação
│   ├── AuthService.cs            # Implementação de autenticação
│   ├── IPaymentService.cs        # Interface de pagamento
│   ├── PaymentService.cs         # Lógica de pagamento
│   └── CarTrackingService.cs     # Background service de rastreamento
├── Dtos/
│   ├── LoginRequestDto.cs        # DTO de login
│   ├── LoginResponseDto.cs       # DTO de resposta de login
│   ├── ChatMessageDto.cs         # DTO de mensagem de chat
│   ├── PaymentRequestDto.cs      # DTO de requisição de pagamento
│   └── PaymentStatusDto.cs       # DTO de status de pagamento
├── Properties/
│   └── launchSettings.json       # Configurações de execução
├── appsettings.json              # Configurações da aplicação
├── appsettings.Development.json  # Configurações de desenvolvimento
├── Program.cs                    # Ponto de entrada e configuração
└── Meetup-WebSocket.csproj       # Arquivo de projeto
```

---

## 🤝 Contribuição

Contribuições são bem-vindas! Siga os passos:

1. Faça um fork do projeto
2. Crie uma branch para sua feature (`git checkout -b feature/NovaFuncionalidade`)
3. Commit suas mudanças (`git commit -m 'Adiciona nova funcionalidade'`)
4. Push para a branch (`git push origin feature/NovaFuncionalidade`)
5. Abra um Pull Request

### Padrões de Código

- Siga as convenções do C# (.NET)
- Use `async/await` para operações assíncronas
- Adicione XML documentation comments em métodos públicos
- Escreva logs informativos para debugging
- Mantenha os Hubs "thin" (lógica de negócio nos Services)

---

## 📝 Licença

Este projeto está sob a licença MIT. Veja o arquivo [LICENSE](LICENSE) para mais detalhes.

---

## 📚 Recursos Adicionais

- [Documentação oficial do SignalR](https://learn.microsoft.com/en-us/aspnet/core/signalr/)
- [JWT Authentication no ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/)
- [Background Services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services)
- [CORS no ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/cors)

---

<div align="center">

**⭐ Se este projeto foi útil, considere dar uma estrela!**

Made with ❤️ and .NET

</div>

