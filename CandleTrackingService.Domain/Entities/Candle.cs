namespace CandleTrackingService.Domain.Entities
{
    public class Candle
    {
        public Guid Id { get; set; }
        public string Symbol { get; set; }
        public DateTime TimeStamp { get; set; }
        public decimal Open { get; set; }
        public decimal High { get; set; }
        public decimal Low { get; set; }
        public decimal Close { get; set; }

        public decimal Volume { get; set; }
        public TimeFrame TimeFrame { get; set; }

        private Candle() { }

        public Candle(
           string symbol,
           DateTime timestamp,
           decimal open,
           decimal high,
           decimal low,
           decimal close,
           decimal volume,
           TimeFrame timeFrame)
        {
            Id = Guid.NewGuid();
            Symbol = symbol ?? throw new ArgumentNullException(nameof(symbol));
            TimeStamp = timestamp;
            Open = open;
            High = high;
            Low = low;
            Close = close;
            Volume = volume;
            TimeFrame = timeFrame;

            Validate();
        }
        private void Validate()
        {
            if (string.IsNullOrWhiteSpace(Symbol))
                throw new ArgumentException("Symbol cannot be empty", nameof(Symbol));

            if (High < Low)
                throw new ArgumentException("High cannot be less than Low");

            if (Open < 0 || High < 0 || Low < 0 || Close < 0 || Volume < 0)
                throw new ArgumentException("Price and volume values cannot be negative");
        }
    }

    public enum TimeFrame
    {
        Minute,
        FiveMinutes,
        FifteenMinutes,
        ThirtyMinutes,
        Hour,
        FourHours,
        Day,
        Week,
        Month
    }
}
