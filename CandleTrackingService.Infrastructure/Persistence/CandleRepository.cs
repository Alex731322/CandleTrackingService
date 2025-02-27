using CandleTrackingService.Domain.Entities;
using CandleTrackingService.Domain.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CandleTrackingService.Infrastructure.Persistence
{
    public class CandleRepository : ICandleRepository
    {
        private readonly CandleDbContext _context;
        public CandleRepository(CandleDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(
            Candle candle, 
            CancellationToken cancellationToken)
        {
            try
            {
                var existingCandle = await _context.Candles
                    .FirstOrDefaultAsync(c => c.Symbol == candle.Symbol &&
                                            c.TimeFrame == candle.TimeFrame &&
                                            c.TimeStamp == candle.TimeStamp,
                                            cancellationToken);
                
                if (existingCandle != null)
                {
                    return;
                }

                await _context.Candles.AddAsync(candle, cancellationToken);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task AddRangeAsync(IEnumerable<Candle> candles, CancellationToken cancellationToken = default)
        {
            try
            {
                var candleList = candles.ToList();

                if(!candleList.Any())
                {
                    return;
                }

                foreach (var candle in candleList)
                {
                    await AddAsync(candle, cancellationToken);
                }

            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Candle> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _context.Candles
                .FirstOrDefaultAsync(c => c.Id == id, cancellationToken);
        }

        public async Task<List<Candle>> GetCandlesAsync(
            string symbol, 
            TimeFrame timeFrame, 
            DateTime from, 
            DateTime to, 
            CancellationToken cancellationToken)
        {
            return await _context.Candles
                .Where(c => c.Symbol == symbol &&
                            c.TimeFrame == timeFrame &&
                            c.TimeStamp >= from &&
                            c.TimeStamp <= to)
                .OrderBy(c => c.TimeStamp)
                .ToListAsync(cancellationToken);
        }

        public async Task<Candle> GetLatestCandleAsync(string symbol, TimeFrame timeFrame, CancellationToken cancellationToken = default)
        {
            return await _context.Candles
                .Where(c => c.Symbol == symbol && 
                            c.TimeFrame == timeFrame)
                .OrderByDescending(c => c.TimeStamp)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                var saved = await _context.SaveChangesAsync(cancellationToken);
                return saved > 0;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
