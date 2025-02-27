using CandleTrackingService.Application.Interfaces;
using CandleTrackingService.Domain.Entities;

namespace CandleTrackingService.Application.Services
{
    public class MarketDataService : IMarketDataService
    {
        // Храним активные подписки
        private readonly Dictionary<string, CancellationTokenSource> _activeSubscriptions = new();

        public async Task<List<Candle>> FetchCandlesAsync(
            string symbol,
            TimeFrame timeFrame,
            DateTime from,
            DateTime to,
            CancellationToken cancellationToken)
        {
            // В реальном проекте здесь будет вызов API биржи
            // return await _apiClient.GetCandlesAsync(symbol, ConvertTimeFrame(timeFrame), from, to, cancellationToken);

            // Для демонстрации возвращаем тестовые данные
            return GenerateSampleCandles(symbol, timeFrame, from, to);
        }

        public async Task SubscribeToRealtimeDataAsync(string symbol, TimeFrame timeFrame, CancellationToken cancellationToken = default)
        {
            var subscriptionKey = GetSubscriptionKey(symbol, timeFrame);

            if (_activeSubscriptions.ContainsKey(subscriptionKey))
            {
                return;
            }

            // Создаем токен отмены для подписки
            var tokenSource = new CancellationTokenSource();
            _activeSubscriptions[subscriptionKey] = tokenSource;

            // Запускаем задачу для получения данных в реальном времени
            // В реальном проекте здесь будет подписка на вебсокет или другой механизм
            _ = Task.Run(async () =>
            {
                try
                {
                    await StreamRealtimeDataAsync(symbol, timeFrame, tokenSource.Token);
                }
                catch (OperationCanceledException)
                {
                }
                catch (Exception ex)
                {
                }
            }, cancellationToken);

            await Task.CompletedTask;
        }

        public async Task UnsubscribeFromRealtimeDataAsync(
            string symbol, 
            TimeFrame timeFrame,
            CancellationToken cancellationToken = default)
        {
            var subscriptionKey = GetSubscriptionKey(symbol, timeFrame);

            if (_activeSubscriptions.TryGetValue(subscriptionKey, out var tokenSource))
            {
               
                // Отменяем задачу получения данных
                tokenSource.Cancel();
                tokenSource.Dispose();
                _activeSubscriptions.Remove(subscriptionKey);
            }

            await Task.CompletedTask;
        }

        private string GetSubscriptionKey(string symbol, TimeFrame timeFrame)
        {
            return $"{symbol}_{timeFrame}";
        }

        private async Task StreamRealtimeDataAsync(string symbol, TimeFrame timeFrame, CancellationToken cancellationToken)
        {
            // В реальном проекте здесь будет логика подписки на потоковые данные
            // Для демонстрации просто имитируем получение данных с периодичностью

            // Определяем интервал обновления в зависимости от таймфрейма
            var updateInterval = GetUpdateInterval(timeFrame);

            while (!cancellationToken.IsCancellationRequested)
            {
                // Имитируем задержку получения данных
                await Task.Delay(updateInterval, cancellationToken);

                if (cancellationToken.IsCancellationRequested)
                    break;

                // Создаем новую свечу с текущим временем
                var candle = GenerateRealtimeCandle(symbol, timeFrame);

                // Здесь должен быть код для отправки свечи в обработчик
                // В реальном проекте использовали бы MediatR или другой механизм сообщений
            }
        }

        private TimeSpan GetUpdateInterval(TimeFrame timeFrame)
        {
            // В реальном проекте интервалы должны быть настраиваемыми
            return timeFrame switch
            {
                TimeFrame.Minute => TimeSpan.FromSeconds(10),
                TimeFrame.FiveMinutes => TimeSpan.FromSeconds(30),
                TimeFrame.FifteenMinutes => TimeSpan.FromMinutes(1),
                TimeFrame.ThirtyMinutes => TimeSpan.FromMinutes(2),
                TimeFrame.Hour => TimeSpan.FromMinutes(5),
                TimeFrame.FourHours => TimeSpan.FromMinutes(10),
                TimeFrame.Day => TimeSpan.FromMinutes(15),
                _ => TimeSpan.FromMinutes(30)
            };
        }

        // Тестовые методы для демонстрации
        private List<Candle> GenerateSampleCandles(string symbol, TimeFrame timeFrame, DateTime from, DateTime to)
        {
            var random = new Random();
            var result = new List<Candle>();
            var price = 100m; // Начальная цена

            var interval = GetCandleInterval(timeFrame);
            var current = from;

            while (current <= to)
            {
                // Генерируем случайное изменение цены
                var priceChange = (decimal)(random.NextDouble() * 2 - 1) * 5;
                var open = price;
                var close = price + priceChange;
                var high = Math.Max(open, close) + (decimal)(random.NextDouble() * 2);
                var low = Math.Min(open, close) - (decimal)(random.NextDouble() * 2);
                var volume = (decimal)(random.NextDouble() * 1000 + 100);

                var candle = new Candle(
                    symbol,
                    current,
                    open,
                    high,
                    low,
                    close,
                    volume,
                    timeFrame);

                result.Add(candle);

                // Обновляем текущую цену для следующей свечи
                price = close;

                // Переходим к следующему интервалу
                current = current.Add(interval);
            }

            return result;
        }

        private Candle GenerateRealtimeCandle(string symbol, TimeFrame timeFrame)
        {
            var random = new Random();
            var timestamp = NormalizeTimestamp(DateTime.UtcNow, timeFrame);

            var basePrice = 100m;
            var priceChange = (decimal)(random.NextDouble() * 2 - 1) * 5;
            var open = basePrice;
            var close = basePrice + priceChange;
            var high = Math.Max(open, close) + (decimal)(random.NextDouble() * 2);
            var low = Math.Min(open, close) - (decimal)(random.NextDouble() * 2);
            var volume = (decimal)(random.NextDouble() * 1000 + 100);

            return new Candle(
                symbol,
                timestamp,
                open,
                high,
                low,
                close,
                volume,
                timeFrame);
        }

        private DateTime NormalizeTimestamp(DateTime timestamp, TimeFrame timeFrame)
        {
            return timeFrame switch
            {
                TimeFrame.Minute => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute, 0, DateTimeKind.Utc),
                TimeFrame.FiveMinutes => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute / 5 * 5, 0, DateTimeKind.Utc),
                TimeFrame.FifteenMinutes => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute / 15 * 15, 0, DateTimeKind.Utc),
                TimeFrame.ThirtyMinutes => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, timestamp.Minute / 30 * 30, 0, DateTimeKind.Utc),
                TimeFrame.Hour => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour, 0, 0, DateTimeKind.Utc),
                TimeFrame.FourHours => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, timestamp.Hour / 4 * 4, 0, 0, DateTimeKind.Utc),
                TimeFrame.Day => new DateTime(timestamp.Year, timestamp.Month, timestamp.Day, 0, 0, 0, DateTimeKind.Utc),
                TimeFrame.Month => new DateTime(timestamp.Year, timestamp.Month, 1, 0, 0, 0, DateTimeKind.Utc),
                _ => timestamp
            };
        }

        private TimeSpan GetCandleInterval(TimeFrame timeFrame)
        {
            return timeFrame switch
            {
                TimeFrame.Minute => TimeSpan.FromMinutes(1),
                TimeFrame.FiveMinutes => TimeSpan.FromMinutes(5),
                TimeFrame.FifteenMinutes => TimeSpan.FromMinutes(15),
                TimeFrame.ThirtyMinutes => TimeSpan.FromMinutes(30),
                TimeFrame.Hour => TimeSpan.FromHours(1),
                TimeFrame.FourHours => TimeSpan.FromHours(4),
                TimeFrame.Day => TimeSpan.FromDays(1),
                TimeFrame.Week => TimeSpan.FromDays(7),
                TimeFrame.Month => TimeSpan.FromDays(30), // Приблизительно
                _ => TimeSpan.FromMinutes(1)
            };
        }


        // Методы необходимые для интеграции с другими микросервисами
        public async Task PublishCandleUpdateAsync(Candle candle, CancellationToken cancellationToken = default)
        {
            // В реальном проекте здесь должна быть отправка сообщения в очередь или брокер сообщений
            // например, через RabbitMQ, Kafka или другой механизм межсервисной коммуникации
        
            // Пример использования MediatR для публикации события
            // await _mediator.Publish(new CandleUpdatedEvent(candle), cancellationToken);

            await Task.CompletedTask;
        }

        public async Task<bool> IsMarketOpenAsync(string symbol, CancellationToken cancellationToken = default)
        {
            // В реальном проекте здесь должна быть проверка статуса рынка через API биржи
            // или через специальный сервис календаря торговых сессий

            // Для демонстрации просто возвращаем true в рабочие часы
            var now = DateTime.UtcNow;
            var isWeekday = now.DayOfWeek != DayOfWeek.Saturday && now.DayOfWeek != DayOfWeek.Sunday;
            var isWorkingHours = now.Hour >= 9 && now.Hour < 17;

            return await Task.FromResult(isWeekday && isWorkingHours);
        }

        public async Task<Dictionary<string, decimal>> GetCurrentPricesAsync(IEnumerable<string> symbols, CancellationToken cancellationToken = default)
        {
            var random = new Random();
            var result = new Dictionary<string, decimal>();

            foreach(var symbol in symbols)
            {
                result[symbol] = 100m + (decimal)(random.NextDouble() * 50);
            }

            return await Task.FromResult(result);
        }


        public async Task CheckHealthAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(100, cancellationToken);
        }

        private string ConvertTimeFrame(TimeFrame timeFrame)
        {
            // Пример конвертации для Binance API
            return timeFrame switch
            {
                TimeFrame.Minute => "1m",
                TimeFrame.FiveMinutes => "5m",
                TimeFrame.FifteenMinutes => "15m",
                TimeFrame.ThirtyMinutes => "30m",
                TimeFrame.Hour => "1h",
                TimeFrame.FourHours => "4h",
                TimeFrame.Day => "1d",
                TimeFrame.Week => "1w",
                TimeFrame.Month => "1M",
                _ => "1m"
            };
        }
    }
}
