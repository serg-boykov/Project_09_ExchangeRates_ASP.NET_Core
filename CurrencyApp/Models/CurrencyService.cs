using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using System;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace CurrencyApp.Models
{
    public class CurrencyService : BackgroundService
    {
        private readonly IMemoryCache _memoryCache;

        public CurrencyService(IMemoryCache memoryCache)
        {
            _memoryCache = memoryCache;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Для фоновой задачи лучше явно указать CultureInfo(),
                    // т.к. в нём может эти настройки отличаться от основного потока,
                    // а значит разделители чисел могут быть разные...
                    // а мы работаем с дробными числами.
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU");

                    // Наш загружаемый xml файл в encoding="windows-1251",
                    // а такая кодировка в ASP.NET Core по умолчанию не доступна.
                    // Если это не сделать, то будет исключение.
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    // Т.к. известно, что будем получать xml файл.
                    XDocument xml = XDocument.Load("https://www.cbr.ru/scripts/XML_daily.asp");

                    CurrencyConverter converter = new CurrencyConverter();

                    // Исходя из структуры xml файла:
                    converter.USD = Convert.ToDecimal(xml.Elements("ValCurs").Elements("Valute")
                        .FirstOrDefault(x => x.Element("NumCode").Value == "840")
                        .Elements("Value").FirstOrDefault().Value);

                    converter.EUR = Convert.ToDecimal(xml.Elements("ValCurs").Elements("Valute")
                        .FirstOrDefault(x => x.Element("NumCode").Value == "978")
                        .Elements("Value").FirstOrDefault().Value);

                    converter.UAH = Convert.ToDecimal(xml.Elements("ValCurs").Elements("Valute")
                        .FirstOrDefault(x => x.Element("NumCode").Value == "980")
                        .Elements("Value").FirstOrDefault().Value);

                    // Используем оперативную память на сервере, чтобы иметь постоянный доступ к данным.
                    _memoryCache.Set("key_currency", converter, TimeSpan.FromMinutes(1440));
                }
                catch (Exception)
                {
                    // logs ...
                }

                // Запуск задачи каждые 6 часов = 6 * 60 минут * 60 секунд * 1000 мс = 21 600 000 мс
                await Task.Delay(21600000, stoppingToken);
            }
        }
    }
}
