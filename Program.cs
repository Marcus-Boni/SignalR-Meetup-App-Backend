using CarTrackingApi.Hubs;
using CarTrackingApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// --- Configura��o dos Servi�os ---

// 1. Adiciona o SignalR ao container de servi�os
builder.Services.AddSignalR();

// 2. Adiciona o servi�o de "motorista" (Game Loop)
builder.Services.AddHostedService<CarTrackingService>();

// 3. Adiciona o servi�o de autentica��o (Clean Architecture)
builder.Services.AddScoped<IAuthService, AuthService>();

// 4. Adiciona o servi�o de pagamento (Clean Architecture - Abstra��o de l�gica de neg�cio)
builder.Services.AddScoped<IPaymentService, PaymentService>();

// 5. Configura��o de Autentica��o JWT
// NOTA: Para ambiente de produ��o, mova a chave secreta para o appsettings.json ou Azure Key Vault
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

    // Configura��o especial para SignalR - Permite enviar o token via query string
    options.Events = new JwtBearerEvents
    {
        OnMessageReceived = context =>
        {
            var accessToken = context.Request.Query["access_token"];

            // Se a requisi��o � para um Hub do SignalR
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
// ISSO � CR�TICO! O Next.js (localhost:3000) vai acessar o .NET (localhost:5xxx)
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

// Adiciona servi�os de API (necess�rio para o builder)
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// --- Configura��o do Pipeline HTTP ---

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

// ATEN��O: A ordem importa!
app.UseRouting();

// 7. Habilita o HTTPS Redirection (pode ser �til)
app.UseHttpsRedirection();

// 8. Usa a pol�tica de CORS que definimos
app.UseCors("AllowNextApp");

// 9. Autentica��o e Autoriza��o (DEVEM vir antes de MapControllers e MapHub)
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 10. Mapeia TODOS os Hubs do SignalR
// - TrackingHub: Rastreamento de carros em tempo real (original)
// - ChatHub: Chat multi-sala
// - PaymentHub: Notifica��es de status de pagamento
app.MapHub<TrackingHub>("/trackingHub");
app.MapHub<ChatHub>("/chatHub");
app.MapHub<PaymentHub>("/paymentHub");

app.Run();