using CandleTrackingService.Domain.Entities;

namespace CandleTrackingService.Application.Interfaces
{
    public interface ICandleService
    {
        Task<List<Candle>> GetHistoricalCandlesAsync(
            string symbol,
            TimeFrame timeFrame,
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken );

        Task StartTrackingAsync(
            string symbol,
            TimeFrame timeFrame,
            CancellationToken cancellationToken);

        Task StopTrackingAsync(
            string symbol,
            TimeFrame timeFrame,
            CancellationToken cancellationToken);

        Task ProccessNewCandleAsync(
            Candle candle,
            CancellationToken cancellationToken);
    }
}
