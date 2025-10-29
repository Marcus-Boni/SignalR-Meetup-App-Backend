# ?? Apresentação - SignalR em Ação

## ?? O que é SignalR?

SignalR é uma biblioteca da Microsoft para comunicação **bidirecional em tempo real** entre servidor e cliente.

### ?? Diferença do HTTP Tradicional:

```
HTTP Tradicional (Request/Response):
Cliente ? Servidor: "Me dê os dados"
Servidor ? Cliente: "Aqui estão"
? Servidor NÃO pode iniciar comunicação

SignalR (WebSockets):
Cliente ? Servidor: Comunicação constante e bidirecional
? Servidor PODE iniciar comunicação
? Notificações em tempo real
? Baixa latência
```

---

## ??? Arquitetura do Projeto

### 3 Demonstrações de SignalR:

| Demo | Hub | Tipo | Autenticação |
|------|-----|------|--------------|
| ?? **Rastreamento de Carro** | `TrackingHub` | Server ? Client | ? Não |
| ?? **Chat Multi-Sala** | `ChatHub` | Client ? Server | ? Sim |
| ?? **Gateway de Pagamento** | `PaymentHub` | Client ? Server | ? Sim |

---

## ?? DEMO 1: Rastreamento de Carro (Server ? Client)

### ?? Conceito:
O **servidor** envia a posição do carro automaticamente, sem que o cliente precise pedir.

### ?? Como Funciona:

#### 1. TrackingHub (Servidor)
```csharp
// Hubs/TrackingHub.cs
public class TrackingHub : Hub
{
    // Hub VAZIO!
    // O servidor é quem manda dados, o cliente só escuta
}
```

**Por que está vazio?**
- Cliente não chama métodos, só **recebe** dados
- É um hub **passivo** (só escuta)

---

#### 2. CarTrackingService (Game Loop)
```csharp
// Services/CarTrackingService.cs
public class CarTrackingService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        while (!stoppingToken.IsCancellationRequested)
        {
            // 1. Calcula nova posição do carro
            var newPosition = CalculateNewPosition();
            
            // 2. ENVIA para TODOS os clientes conectados
            await _hubContext.Clients.All.SendAsync(
                "ReceiveCarPosition", 
                newPosition
            );
            
            // 3. Aguarda 100ms (10 frames/segundo)
            await Task.Delay(100, stoppingToken);
        }
    }
}
```

**Explicação:**
- `BackgroundService`: Roda em loop infinito no servidor
- `Clients.All`: Envia para **todos** os conectados
- `SendAsync("ReceiveCarPosition", ...)`: Nome do evento que o cliente vai escutar
- **Game Loop**: Atualiza a cada 100ms (10 FPS)

---

#### 3. Frontend (Cliente React/Next.js)
```typescript
// hooks/useCarTracking.ts
const connection = new HubConnectionBuilder()
  .withUrl("https://localhost:7xxx/trackingHub")
  .build();

// Escuta o evento "ReceiveCarPosition"
connection.on("ReceiveCarPosition", (position) => {
  console.log("Posição recebida:", position);
  setCarPosition(position); // Atualiza o estado React
});

await connection.start();
```

**Explicação:**
- Cliente se **conecta** ao hub
- Cliente **escuta** o evento `ReceiveCarPosition`
- Quando recebe, atualiza a UI automaticamente

---

### ?? O que Mostrar na Demo:

1. ? Abrir a tela de rastreamento
2. ? Mostrar o carro se movendo automaticamente
3. ? Abrir o console do navegador ? Ver os logs de posição chegando
4. ? Abrir em **2 abas** ? Ambas veem o mesmo carro ao mesmo tempo
5. ? Explicar: "O servidor está enviando dados sem ninguém pedir"

---

## ?? DEMO 2: Chat Multi-Sala (Client ? Server)

### ?? Conceito:
Cliente **envia** mensagens, servidor **distribui** para outros usuários na mesma sala.

### ?? Como Funciona:

#### 1. ChatHub (Servidor)
```csharp
[Authorize] // ?? Requer autenticação JWT
public class ChatHub : Hub
{
    // Cliente chama: JoinRoom("sala1")
    public async Task JoinRoom(string roomName)
    {
        // Adiciona o usuário ao grupo "sala1"
        await Groups.AddToGroupAsync(Context.ConnectionId, roomName);
        
        // Notifica a sala: "João entrou"
        await Clients.Group(roomName).SendAsync(
            "UserJoined", 
            Context.User.Identity.Name, 
            roomName
        );
    }

    // Cliente chama: SendMessage("sala1", "Olá!")
    public async Task SendMessage(string roomName, string message)
    {
        var username = Context.User.Identity.Name;
        
        // Envia a mensagem APENAS para a sala1
        await Clients.Group(roomName).SendAsync(
            "ReceiveMessage",
            username,
            message,
            DateTime.UtcNow
        );
    }
}
```

**Conceitos Importantes:**
- `[Authorize]`: Hub só funciona com token JWT válido
- `Groups`: Isola mensagens por sala (sala1, sala2, etc.)
- `Context.User.Identity.Name`: Pega o username do token JWT
- `Clients.Group(roomName)`: Envia só para usuários da sala

---

#### 2. Frontend (Cliente)
```typescript
// hooks/useChatHub.ts
const connection = new HubConnectionBuilder()
  .withUrl(`${API_URL}/chatHub`, {
    accessTokenFactory: () => token, // ?? Envia o JWT
  })
  .build();

// Escuta mensagens recebidas
connection.on("ReceiveMessage", (username, message, timestamp) => {
  console.log(`${username}: ${message}`);
  setMessages(prev => [...prev, { username, message, timestamp }]);
});

// Entra na sala
await connection.invoke("JoinRoom", "sala1");

// Envia mensagem
await connection.invoke("SendMessage", "sala1", "Olá pessoal!");
```

**Explicação:**
- `accessTokenFactory`: Passa o token JWT na conexão
- `connection.on`: **Recebe** eventos do servidor
- `connection.invoke`: **Chama** métodos do servidor

---

### ?? O que Mostrar na Demo:

1. ? Fazer login com `admin` / `admin123`
2. ? Entrar na sala "geral"
3. ? Enviar uma mensagem
4. ? Abrir em **2 abas** com **usuários diferentes**
5. ? Mostrar que a mensagem aparece **instantaneamente** na outra aba
6. ? Trocar de sala ? Mostrar que mensagens ficam isoladas por sala
7. ? Abrir o console ? Mostrar os logs de `ReceiveMessage`

---

## ?? DEMO 3: Gateway de Pagamento (HTTP + SignalR)

### ?? Conceito:
Cliente inicia um pagamento via **HTTP POST**, e recebe atualizações de status via **SignalR**.

### ?? Como Funciona:

#### Fluxo Completo:
```
1. Cliente ? HTTP POST /api/payments/pay
   Body: { "orderId": "123", "amount": 99.90 }
   
2. Servidor ? Responde imediatamente:
   { "message": "Pagamento iniciado", "orderId": "123" }
   
3. Cliente ? SignalR: Invoke("SubscribeToPaymentStatus", "123")
   
4. Servidor ? Processa pagamento em background (5 segundos)
   
5. Servidor ? SignalR: SendAsync("PaymentStatusUpdate", ...)
   Status: "Processing" ? "Approved" ou "Rejected"
   
6. Cliente ? Recebe notificações em tempo real
```

---

#### 1. Controller (HTTP)
```csharp
[Authorize]
[ApiController]
[Route("api/payments")]
public class PaymentsController : ControllerBase
{
    [HttpPost("pay")]
    public async Task<IActionResult> ProcessPayment([FromBody] PaymentRequestDto request)
    {
        var userId = User.Identity?.Name;
        
        // Inicia o processamento em BACKGROUND
        _ = _paymentService.ProcessPaymentAsync(request, userId);
        
        // Responde IMEDIATAMENTE (não espera o resultado)
        return Ok(new {
            Message = "Pagamento iniciado. Você receberá atualizações via SignalR.",
            OrderId = request.OrderId
        });
    }
}
```

**Por que não esperar o resultado?**
- Pagamentos podem demorar (5-30 segundos)
- HTTP tem timeout
- Cliente recebe updates via SignalR em tempo real

---

#### 2. PaymentHub (SignalR)
```csharp
[Authorize]
public class PaymentHub : Hub
{
    // Cliente se inscreve para receber updates do pedido "123"
    public async Task SubscribeToPaymentStatus(string orderId)
    {
        var userId = Context.User.Identity.Name;
        
        // Cria grupo privado: "payment-status-admin-123"
        var groupName = $"payment-status-{userId}-{orderId}";
        
        await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
        
        // Confirma a inscrição
        await Clients.Caller.SendAsync("SubscriptionConfirmed", new {
            OrderId = orderId,
            Message = "Você está inscrito para updates"
        });
    }
}
```

**Por que usar grupos?**
- **Segurança**: Cada usuário só vê os próprios pagamentos
- **Escalabilidade**: Não envia para todos os clientes

---

#### 3. PaymentService (Background)
```csharp
public async Task ProcessPaymentAsync(PaymentRequestDto request, string userId)
{
    var groupName = $"payment-status-{userId}-{request.OrderId}";
    
    // FASE 1: Status "Processing"
    await _hubContext.Clients.Group(groupName).SendAsync(
        "PaymentStatusUpdate",
        new { Status = "Processing", Message = "Processando..." }
    );
    
    // Simula processamento (5 segundos)
    await Task.Delay(5000);
    
    // FASE 2: Status "Approved" ou "Rejected" (80% de aprovação)
    var isApproved = Random.Shared.Next(100) < 80;
    
    await _hubContext.Clients.Group(groupName).SendAsync(
        "PaymentStatusUpdate",
        new {
            Status = isApproved ? "Approved" : "Rejected",
            Message = isApproved 
                ? "Pagamento aprovado!" 
                : "Pagamento recusado."
        }
    );
}
```

---

#### 4. Frontend (Cliente)
```typescript
// 1. Conecta ao PaymentHub
const paymentConnection = new HubConnectionBuilder()
  .withUrl(`${API_URL}/paymentHub`, {
    accessTokenFactory: () => token,
  })
  .build();

// 2. Escuta updates
paymentConnection.on("PaymentStatusUpdate", (status) => {
  console.log("Status:", status);
  setPaymentStatus(status.Status); // Processing ? Approved/Rejected
});

await paymentConnection.start();

// 3. Inicia o pagamento via HTTP
const response = await fetch(`${API_URL}/api/payments/pay`, {
  method: "POST",
  headers: {
    "Authorization": `Bearer ${token}`,
    "Content-Type": "application/json",
  },
  body: JSON.stringify({ orderId: "123", amount: 99.90 }),
});

// 4. Inscreve para receber updates
await paymentConnection.invoke("SubscribeToPaymentStatus", "123");

// 5. Aguarda as notificações (Processing ? Approved/Rejected)
```

---

### ?? O que Mostrar na Demo:

1. ? Fazer login
2. ? Entrar na tela de pagamento
3. ? Preencher: OrderId="123", Amount=99.90
4. ? Clicar em "Pagar"
5. ? Mostrar o status mudando em tempo real:
   - "Processing" (imediato)
   - "Approved" ou "Rejected" (após 5 segundos)
6. ? Abrir o console ? Mostrar os eventos chegando
7. ? Explicar: "O pagamento é processado em background, mas o usuário vê o progresso ao vivo"

---

## ?? Autenticação JWT

### Como Funciona:

1. **Login via HTTP:**
```bash
POST /api/auth/login
Body: { "username": "admin", "password": "admin123" }

Response: { 
  "token": "eyJhbGciOiJI...", 
  "username": "admin" 
}
```

2. **Usando o Token:**
```typescript
// HTTP
fetch("/api/payments/pay", {
  headers: { "Authorization": "Bearer " + token }
});

// SignalR
new HubConnectionBuilder()
  .withUrl("/chatHub", {
    accessTokenFactory: () => token
  });
```

3. **No Servidor:**
```csharp
[Authorize] // ?? Valida o token automaticamente
public class ChatHub : Hub
{
    // Context.User.Identity.Name ? "admin"
}
```

---

## ?? Program.cs - Configuração

### Ordem IMPORTANTÍSSIMA:
```csharp
app.UseRouting();        // 1. Define as rotas
app.UseCors("...");      // 2. Permite frontend se conectar
app.UseAuthentication(); // 3. Valida JWT
app.UseAuthorization();  // 4. Verifica permissões
app.MapHub<ChatHub>();   // 5. Mapeia os hubs
```

**Se inverter a ordem ? Autenticação não funciona!**

---

## ?? Comparação dos 3 Demos

| Aspecto | Rastreamento | Chat | Pagamento |
|---------|--------------|------|-----------|
| **Iniciativa** | Servidor ? Cliente | Cliente ? Servidor | HTTP + SignalR |
| **Autenticação** | ? Não | ? Sim | ? Sim |
| **Grupos** | ? Não (broadcast) | ? Sim (salas) | ? Sim (privado) |
| **Use Case** | Dashboards, GPS | Chat, Colaboração | Notificações, Status |
| **Background** | ? BackgroundService | ? Não | ? Task em background |

---

## ?? Roteiro de Apresentação

### 1. Introdução (2 min)
- "SignalR permite comunicação em tempo real"
- "Hoje vamos ver 3 cenários reais"

### 2. Demo 1 - Rastreamento (3 min)
- Mostrar o carro se movendo
- Abrir 2 abas ? Sincronização
- Explicar: "Servidor envia sem ninguém pedir"

### 3. Demo 2 - Chat (5 min)
- Fazer login
- Enviar mensagens
- Abrir 2 usuários diferentes
- Explicar: "Mensagens isoladas por sala"

### 4. Demo 3 - Pagamento (5 min)
- Fazer login
- Iniciar pagamento
- Mostrar status em tempo real
- Explicar: "HTTP inicia, SignalR notifica"

### 5. Conclusão (2 min)
- Recap dos 3 padrões
- Quando usar SignalR vs HTTP

---

## ?? Dicas para a Apresentação

### ? FAÇA:
- Abra o **console do navegador** ? Mostre os logs
- Use **2 abas/navegadores** ? Mostre a sincronização
- Explique **por que** cada decisão foi tomada
- Mostre o **código relevante** rapidamente

### ? NÃO FAÇA:
- Não leia o código linha por linha
- Não entre em detalhes de implementação (tokens, JWT, etc.)
- Não demonstre bugs ou erros (teste antes!)
- Não use jargões sem explicar

---

## ?? Pontos-Chave para Memorizar

1. **SignalR = Comunicação bidirecional em tempo real**
2. **Hub = Ponto de comunicação (como um "controller" do SignalR)**
3. **Clients.All = Broadcast para todos**
4. **Clients.Group = Envia apenas para um grupo**
5. **Clients.Caller = Envia apenas para quem chamou**
6. **[Authorize] = Requer autenticação JWT**
7. **Grupos = Isolamento de mensagens (segurança + performance)**
8. **BackgroundService = Loop infinito no servidor**

---

## ?? Perguntas Comuns

### "SignalR é melhor que HTTP?"
? Não! São complementares:
- **HTTP**: CRUD, uploads, downloads
- **SignalR**: Notificações, atualizações em tempo real

### "SignalR funciona em produção?"
? Sim! Microsoft Teams, Slack, WhatsApp Web usam tecnologias similares.

### "É difícil escalar?"
?? Sim, mas o Azure SignalR Service resolve isso automaticamente.

---

**?? Boa sorte na apresentação!**
