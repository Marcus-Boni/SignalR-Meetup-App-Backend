using CarTrackingApi.Hubs;
using CarTrackingApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Configuração dos Serviços ---

// 1. Adiciona o SignalR ao container de serviços
builder.Services.AddSignalR();

// 2. Adiciona o serviço de "motorista" (Game Loop)
builder.Services.AddHostedService<CarTrackingService>();

// 3. Adiciona o serviço de autenticação (Clean Architecture)
builder.Services.AddScoped<IAuthService, AuthService>();

// 4. Adiciona o serviço de pagamento (Clean Architecture - Abstração de lógica de negócio)
builder.Services.AddScoped<IPaymentService, PaymentService>();

// 5. Configuração de Autenticação JWT
// NOTA: Para ambiente de produção, mova a chave secreta para o appsettings.json ou Azure Key Vault
var jwtKey = builder.Configuration["Jwt:Key"] ?? "SuperSecretKeyForDevelopmentOnly_ChangeInProduction_MinimumLength32Characters!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "CarTrackingApi";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "CarTrackingApiClients";

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
    };

    // Configuração especial para SignalR - Permite enviar o token via query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // Se a requisição é para um Hub do SignalR
            var path = context.HttpContext.Request.Path;
            if (!string.IsNullOrEmpty(accessToken) &&
                (path.StartsWithSegments("/trackingHub") ||
                 path.StartsWithSegments("/chatHub") ||
                 path.StartsWithSegments("/paymentHub")))
            {
                context.Token = accessToken;
            }

            return Task.CompletedTask;
        }
    };
});

builder.Services.AddAuthorization();

// 6. Adiciona o CORS (Cross-Origin Resource Sharing)
// ISSO É CRÍTICO! O Next.js (localhost:3000) vai acessar o .NET (localhost:5xxx)
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowNextApp",
        policy =>
        {
            policy.WithOrigins("http://localhost:3000") // A porta do seu app Next.js
                  .AllowAnyHeader()
                  .AllowAnyMethod()
                  .AllowCredentials(); // Essencial para SignalR
        });
});

// Adiciona serviços de API (necessário para o builder)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Configuração do Pipeline HTTP ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ATENÇÃO: A ordem importa!
app.UseRouting();

// 7. Habilita o HTTPS Redirection (pode ser útil)
app.UseHttpsRedirection();

// 8. Usa a política de CORS que definimos
app.UseCors("AllowNextApp");

// 9. Autenticação e Autorização (DEVEM vir antes de MapControllers e MapHub)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 10. Mapeia TODOS os Hubs do SignalR
// - TrackingHub: Rastreamento de carros em tempo real (original)
// - ChatHub: Chat multi-sala
// - PaymentHub: Notificações de status de pagamento
app.MapHub<TrackingHub>("/trackingHub");
app.MapHub<ChatHub>("/chatHub");
app.MapHub<PaymentHub>("/paymentHub");

app.Run();