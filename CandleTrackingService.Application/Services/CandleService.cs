using CandleTrackingService.Application.Interfaces;
using CandleTrackingService.Domain.Entities;
using CandleTrackingService.Domain.Repositories;

namespace CandleTrackingService.Application.Services
{
    public class CandleService : ICandleService
    {
        private readonly IMarketDataService _marketDataService;
        private readonly ICandleRepository _candleRepository;
        //private readonly ILogger<CandleService> _logger;

        public CandleService(
            IMarketDataService marketDataService,
            ICandleRepository candleRepository)
        {
            _marketDataService = marketDataService;
            _candleRepository = candleRepository;
        }

        public async Task<List<Candle>> GetHistoricalCandlesAsync
            (string symbol, 
            TimeFrame timeFrame, 
            DateTime from, 
            DateTime to, 
            CancellationToken cancellationToken)
        {
            var candles = await _candleRepository.GetCandlesAsync(symbol, timeFrame, from, to, cancellationToken);
           
            // Если данных нет или они неполные, дозапрашиваем через API
            if (candles.Count == 0 || !AreCandlesComplete(candles, timeFrame, from, to))
            {
                var fetchedCandles = await _marketDataService.FetchCandlesAsync(symbol, timeFrame, from, to, cancellationToken);


                await _candleRepository.AddRangeAsync(fetchedCandles, cancellationToken);
                await _candleRepository.SaveChangesAsync(cancellationToken);

                // Обновляем результат
                candles = await _candleRepository.GetCandlesAsync(symbol, timeFrame, from, to, cancellationToken);
            }
            
            return candles;
        }

      

        public async Task StartTrackingAsync(
            string symbol, 
            TimeFrame timeFrame, 
            CancellationToken cancellationToken)
        {
            await _marketDataService.SubscribeToRealtimeDataAsync(symbol, timeFrame, cancellationToken);
        }

        public async Task StopTrackingAsync(
            string symbol,
            TimeFrame timeFrame,
            CancellationToken cancellationToken)
        {
            await _marketDataService.UnsubscribeFromRealtimeDataAsync(symbol, timeFrame, cancellationToken);
        }

        public async Task ProccessNewCandleAsync(Candle candle, CancellationToken cancellationToken)
        {
            // Сохраняем новую свечу
            await _candleRepository.AddAsync(candle, cancellationToken);
            await _candleRepository.SaveChangesAsync(cancellationToken);
        }

        private bool AreCandlesComplete(List<Candle> candles, TimeFrame timeFrame, DateTime from, DateTime to)
        {
            // Логика проверки полноты данных
            // Это упрощенная реализация. В реальном проекте нужна более сложная логика,
            // учитывающая выходные, праздники и торговые часы

            // Вычисляем ожидаемое количество свечей
            var expectedCount = CalculateExpectedCandleCount(timeFrame, from, to);

            return candles.Count >= expectedCount;
        }


        private int CalculateExpectedCandleCount(TimeFrame timeFrame, DateTime from, DateTime to)
        {
            var timeSpan = to - from;

            return timeFrame switch
            {
                TimeFrame.Minute => (int)timeSpan.TotalMinutes,
                TimeFrame.FiveMinutes => (int)(timeSpan.TotalMinutes / 5),
                TimeFrame.FifteenMinutes => (int)(timeSpan.TotalMinutes / 15),
                TimeFrame.ThirtyMinutes => (int)(timeSpan.TotalMinutes / 30),
                TimeFrame.Hour => (int)timeSpan.TotalHours,
                TimeFrame.FourHours => (int)(timeSpan.TotalHours / 4),
                TimeFrame.Day => (int)timeSpan.TotalDays,
                TimeFrame.Week => (int)(timeSpan.TotalDays / 7),
                TimeFrame.Month => (int)(timeSpan.TotalDays / 30),
                _ => throw new ArgumentOutOfRangeException(nameof(timeFrame), timeFrame, null)
            };
        }
    }
}
