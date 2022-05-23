using System;

namespace CurrencyApp.Models
{
    public class CurrencyConverter
    {
        public decimal USD { get; set; }
        public decimal ConvertToUSD(decimal priceRUB) => Decimal.Round(priceRUB / USD, 2);

        public decimal EUR { get; set; }
        public decimal ConvertToEUR(decimal priceRUB) => Decimal.Round(priceRUB / EUR, 2);

        // Nominal for KZT = 100 (According to the Central Bank of Russia)
        public decimal KZT { get; set; }
        public decimal ConvertToKZT(decimal priceRUB) => Decimal.Round(priceRUB / (KZT / 100), 2);
    }
}
