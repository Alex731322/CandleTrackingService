using CandleTrackingService.Domain.Entities;

namespace CandleTrackingService.Application.Interfaces
{
    public interface IMarketDataService
    {
        Task<List<Candle>> FetchCandlesAsync(
            string symbol,
            TimeFrame timeFrame,
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken );
        Task SubscribeToRealtimeDataAsync(
            string symbol,
            TimeFrame timeFrame,
            CancellationToken cancellationToken = default);

        Task UnsubscribeFromRealtimeDataAsync(
            string symbol,
            TimeFrame timeFrame,
            CancellationToken cancellationToken = default);
    }
}
