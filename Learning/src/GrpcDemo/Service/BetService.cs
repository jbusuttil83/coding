using Grpc.Core;

namespace GrpcDemo.Service
{
    public class BetService : Bet.BetBase
    {
        public override Task<PlaceBetResponse> PlaceBet(PlaceBetRequest request, ServerCallContext context)
        {
            var randomInt = new Random().Next(1000, 9999);

            Thread.Sleep(randomInt);
            
            return Task.FromResult(new PlaceBetResponse
            {
                BetId = randomInt
            });
        }
    }    
}