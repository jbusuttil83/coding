using Grpc.Net.Client;

namespace GrpcDemo;

public class BetClient : BackgroundService
{
    private readonly ILogger<BetClient> _logger;
    private readonly string _url;

    public BetClient(ILogger<BetClient> logger, IConfiguration configuration)
    {
        _logger = logger;
        _url = configuration["Kestrel:Endpoints:gRPC:Url"];
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        using (var channel = GrpcChannel.ForAddress(_url))
        {
            var client = new Bet.BetClient(channel);

            while (!stoppingToken.IsCancellationRequested)
            {
                var response = await client.PlaceBetAsync(new PlaceBetRequest
                {
                    Amount = 12,
                    Currency = "EUR"
                });
                
                _logger.LogInformation("Place Bet {BetId}", response.BetId);;
            }
        }
    }
}