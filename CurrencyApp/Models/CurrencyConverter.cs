namespace CurrencyApp.Models
{
    public class CurrencyConverter
    {
        public decimal USD { get; set; }
        public decimal ConvertToUSD(decimal priceRUB) => priceRUB / USD;

        public decimal EUR { get; set; }
        public decimal ConvertToEUR(decimal priceRUB) => priceRUB / EUR;

        // Nominal for KZT = 100 (According to the Central Bank of Russia)
        public decimal KZT { get; set; }
        public decimal ConvertToKZT(decimal priceRUB) => priceRUB / (KZT / 100);
    }
}
