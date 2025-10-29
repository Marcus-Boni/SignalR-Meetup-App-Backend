// Services/CarTrackingService.cs
using Microsoft.AspNetCore.SignalR;
using CarTrackingApi.Hubs;

namespace CarTrackingApi.Services
{
    /// <summary>
    /// Representa a posição detalhada do veículo
    /// </summary>
    public class VehiclePosition
    {
        public double X { get; set; }
        public double Y { get; set; }
        public double Speed { get; set; } // km/h
        public double Heading { get; set; } // Direção em graus (0-360)
        public string Status { get; set; } = "Moving"; // Moving, Stopped, Accelerating, Braking
        public DateTime Timestamp { get; set; }
        public int RouteProgress { get; set; } // Percentual da rota completa (0-100)
    }

    /// <summary>
    /// Serviço de background que simula o rastreamento de um veículo em tempo real
    /// com movimentação suave e realista
    /// </summary>
    public class CarTrackingService : BackgroundService
    {
        private readonly IHubContext<TrackingHub> _hubContext;
        private readonly ILogger<CarTrackingService> _logger;

        // Configurações do mapa (ajuste conforme o tamanho do seu canvas no frontend)
        private const int MAP_WIDTH = 800;
        private const int MAP_HEIGHT = 600;

        // Rota realista de um veículo de entrega urbano
        // Simula uma rota por um bairro: Partida -> Loja 1 -> Loja 2 -> Loja 3 -> Retorno
        private readonly Position[] _routeWaypoints =
        [
            // Partida - Depósito (canto inferior esquerdo)
            new Position { X = 50, Y = 550, Name = "Depósito Central" },
            
            // Rua Principal - Indo para o norte
            new Position { X = 50, Y = 450 },
            new Position { X = 50, Y = 350 },
            new Position { X = 50, Y = 250 },
            
            // Curva para a direita
            new Position { X = 100, Y = 200 },
            new Position { X = 150, Y = 180 },
            
            // Primeira Entrega - Rua Comercial Norte
            new Position { X = 250, Y = 180, Name = "Loja A - Rua Norte" },
            
            // Continua pela rua comercial
            new Position { X = 350, Y = 180 },
            new Position { X = 450, Y = 180 },
            
            // Curva para baixo (direita)
            new Position { X = 500, Y = 200 },
            new Position { X = 550, Y = 250 },
            
            // Segunda Entrega - Área Residencial Leste
            new Position { X = 550, Y = 350, Name = "Loja B - Zona Residencial" },
            
            // Desce mais
            new Position { X = 550, Y = 450 },
            
            // Curva para a esquerda
            new Position { X = 520, Y = 500 },
            new Position { X = 450, Y = 520 },
            
            // Terceira Entrega - Shopping Sul
            new Position { X = 350, Y = 520, Name = "Loja C - Shopping Sul" },
            
            // Retorno ao depósito
            new Position { X = 250, Y = 520 },
            new Position { X = 150, Y = 520 },
            new Position { X = 100, Y = 540 },
            
            // Volta ao início
            new Position { X = 50, Y = 550, Name = "Retorno ao Depósito" }
        ];

        private int _currentWaypointIndex = 0;
        private double _progressToNextWaypoint = 0.0;
        private readonly Random _random = new();

        // Estado do veículo
        private double _currentSpeed = 0.0; // km/h
        private const double MAX_SPEED = 60.0; // km/h
        private const double ACCELERATION = 2.0; // km/h por tick
        private const double DECELERATION = 3.0; // km/h por tick
        
        // Controle de paradas
        private DateTime? _stopUntil = null; // Tempo até quando o veículo deve ficar parado

        public CarTrackingService(
            IHubContext<TrackingHub> hubContext,
            ILogger<CarTrackingService> logger)
        {
            _hubContext = hubContext;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("🚗 Serviço de rastreamento de veículo iniciado");
            _logger.LogInformation("📍 Rota configurada com {WaypointCount} pontos de referência", _routeWaypoints.Length);

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Calcula a posição atual com interpolação suave
                    var position = CalculateCurrentPosition();

                    // Envia a posição para todos os clientes conectados
                    await _hubContext.Clients.All.SendAsync("ReceivePosition", position, stoppingToken);

                    // Atualiza o progresso para o próximo ponto
                    UpdateProgress();

                    // Controla a velocidade de atualização (50ms = 20 frames por segundo)
                    await Task.Delay(50, stoppingToken);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "❌ Erro ao processar posição do veículo");
                }
            }

            _logger.LogInformation("🛑 Serviço de rastreamento de veículo encerrado");
        }

        /// <summary>
        /// Calcula a posição atual do veículo com interpolação suave entre waypoints
        /// </summary>
        private VehiclePosition CalculateCurrentPosition()
        {
            var currentWaypoint = _routeWaypoints[_currentWaypointIndex];
            var nextWaypoint = _routeWaypoints[(_currentWaypointIndex + 1) % _routeWaypoints.Length];

            // Interpolação linear entre os dois pontos
            var x = Lerp(currentWaypoint.X, nextWaypoint.X, _progressToNextWaypoint);
            var y = Lerp(currentWaypoint.Y, nextWaypoint.Y, _progressToNextWaypoint);

            // Calcula o ângulo de direção (heading)
            var heading = CalculateHeading(currentWaypoint, nextWaypoint);

            // Determina o status do veículo
            var status = DetermineVehicleStatus();

            // Calcula o progresso total da rota
            var routeProgress = (int)(((_currentWaypointIndex + _progressToNextWaypoint) / _routeWaypoints.Length) * 100);

            return new VehiclePosition
            {
                X = x,
                Y = y,
                Speed = Math.Round(_currentSpeed, 1),
                Heading = heading,
                Status = status,
                Timestamp = DateTime.UtcNow,
                RouteProgress = routeProgress
            };
        }

        /// <summary>
        /// Atualiza o progresso e a velocidade do veículo
        /// </summary>
        private void UpdateProgress()
        {
            // Verifica se o veículo está em parada programada
            if (_stopUntil.HasValue)
            {
                if (DateTime.UtcNow < _stopUntil.Value)
                {
                    // Ainda está na parada, mantém velocidade zero
                    _currentSpeed = 0;
                    return;
                }
                else
                {
                    // Fim da parada
                    _stopUntil = null;
                    _logger.LogInformation("🚀 Veículo retomando movimento");
                }
            }

            var currentWaypoint = _routeWaypoints[_currentWaypointIndex];
            var nextWaypoint = _routeWaypoints[(_currentWaypointIndex + 1) % _routeWaypoints.Length];

            // Verifica se o próximo waypoint é um ponto de parada (tem nome)
            var isApproachingStop = !string.IsNullOrEmpty(nextWaypoint.Name);
            var distanceToNext = 1.0 - _progressToNextWaypoint;

            // Controle de velocidade
            if (isApproachingStop && distanceToNext < 0.2)
            {
                // ✅ CORRIGIDO: Desacelera mas mantém velocidade mínima para continuar movendo
                var minSpeed = 10.0; // Velocidade mínima para continuar
                _currentSpeed = Math.Max(minSpeed, _currentSpeed - DECELERATION);
            }
            else if (_currentSpeed < MAX_SPEED)
            {
                // Acelera normalmente
                var targetSpeed = isApproachingStop ? MAX_SPEED * 0.6 : MAX_SPEED;
                _currentSpeed = Math.Min(targetSpeed, _currentSpeed + ACCELERATION);
            }

            // Simula variação de velocidade realista (trânsito)
            if (_random.Next(0, 100) < 2) // 2% de chance por tick
            {
                _currentSpeed *= 0.9; // Reduz 10% (obstáculo/semáforo)
            }

            // ✅ GARANTIA: Velocidade mínima quando não está em parada programada
            if (!_stopUntil.HasValue && _currentSpeed < 5.0)
            {
                _currentSpeed = 5.0; // Sempre mantém movimento mínimo
            }

            // Calcula a distância percorrida neste tick
            // Normaliza a velocidade para o progresso (0.0 a 1.0)
            var distance = CalculateDistance(currentWaypoint, nextWaypoint);
            var speedFactor = (_currentSpeed / MAX_SPEED) * 0.02; // Ajusta a velocidade de progressão

            _progressToNextWaypoint += speedFactor;

            // Se chegou ao próximo waypoint
            if (_progressToNextWaypoint >= 1.0)
            {
                _progressToNextWaypoint = 0.0;
                _currentWaypointIndex = (_currentWaypointIndex + 1) % _routeWaypoints.Length;

                var arrivedAt = _routeWaypoints[_currentWaypointIndex];
                if (!string.IsNullOrEmpty(arrivedAt.Name))
                {
                    _logger.LogInformation("📍 Veículo chegou em: {Location}", arrivedAt.Name);
                    
                    // ✅ CORRIGIDO: Agenda a parada sem bloquear
                    _currentSpeed = 0;
                    _stopUntil = DateTime.UtcNow.AddSeconds(2);
                    _logger.LogInformation("🛑 Veículo parado por 2 segundos");
                }
            }
        }

        /// <summary>
        /// Determina o status atual do veículo
        /// </summary>
        private string DetermineVehicleStatus()
        {
            if (_currentSpeed < 1.0)
                return "Stopped";
            
            var currentWaypoint = _routeWaypoints[_currentWaypointIndex];
            var nextWaypoint = _routeWaypoints[(_currentWaypointIndex + 1) % _routeWaypoints.Length];
            var isApproachingStop = !string.IsNullOrEmpty(nextWaypoint.Name);
            var distanceToNext = 1.0 - _progressToNextWaypoint;

            if (isApproachingStop && distanceToNext < 0.3)
                return "Braking";
            
            if (_currentSpeed < MAX_SPEED * 0.5)
                return "Accelerating";
            
            return "Moving";
        }

        /// <summary>
        /// Interpolação linear entre dois valores
        /// </summary>
        private static double Lerp(double start, double end, double t)
        {
            return start + (end - start) * t;
        }

        /// <summary>
        /// Calcula a direção (heading) em graus
        /// </summary>
        private static double CalculateHeading(Position from, Position to)
        {
            var dx = to.X - from.X;
            var dy = to.Y - from.Y;
            var radians = Math.Atan2(dy, dx);
            var degrees = radians * (180.0 / Math.PI);
            
            // Normaliza para 0-360
            return (degrees + 360) % 360;
        }

        /// <summary>
        /// Calcula a distância euclidiana entre dois pontos
        /// </summary>
        private static double CalculateDistance(Position from, Position to)
        {
            var dx = to.X - from.X;
            var dy = to.Y - from.Y;
            return Math.Sqrt(dx * dx + dy * dy);
        }

        /// <summary>
        /// Classe auxiliar para posição básica
        /// </summary>
        public class Position
        {
            public double X { get; set; }
            public double Y { get; set; }
            public string? Name { get; set; }
        }
    }
}