using CandleTrackingService.Domain.Entities;

namespace CandleTrackingService.Domain.Repositories
{
    public interface ICandleRepository
    {
        Task<Candle> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<List<Candle>> GetCandlesAsync(
            string symbol,
            TimeFrame timeFrame,
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken = default);
        Task<Candle> GetLatestCandleAsync(
            string symbol,
            TimeFrame timeFrame,
            CancellationToken cancellationToken = default);
        Task AddAsync(Candle candle, CancellationToken cancellationToken = default);
        Task AddRangeAsync(IEnumerable<Candle> candles, CancellationToken cancellationToken = default);
        Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default);
    }
}
