using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
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
        private readonly ILogger<CurrencyService> _logger;

        public CurrencyService(IMemoryCache memoryCache, ILogger<CurrencyService> logger)
        {
            _memoryCache = memoryCache;
            _logger = logger; 
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    // Since this task is executed in a different thread,
                    // it is likely that the default culture may differ
                    // from the one set in our application,
                    // so we will explicitly indicate the one we need so
                    // that there are no problems with separators, names, etc.
                    Thread.CurrentThread.CurrentCulture = new CultureInfo("ru-RU"); // <== the culture you want.

                    // Encoding of the xml file from the site of the Central Bank of Russia ==
                    // windows-1251 by default it is not available in .NET Core,
                    // so we register the required provider.
                    Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);

                    // Because we know that the data comes to us in a file, in XML format,
                    // so there is no need to use WebRequest,
                    // we use the XDocument class in our work and
                    // immediately pick up the file from a remote server.
                    XDocument xml = XDocument.Load("https://www.cbr.ru/scripts/XML_daily.asp");

                    CurrencyConverter converter = new()
                    {
                        // Then we parse the file and find the currencies we need by their ID code,
                        // and fill in the model class:
                        USD = Convert.ToDecimal(xml.Elements("ValCurs").Elements("Valute")
                        .FirstOrDefault(x => x.Element("NumCode").Value == "840")
                        .Elements("Value").FirstOrDefault().Value),

                        EUR = Convert.ToDecimal(xml.Elements("ValCurs").Elements("Valute")
                        .FirstOrDefault(x => x.Element("NumCode").Value == "978")
                        .Elements("Value").FirstOrDefault().Value),

                        KZT = Convert.ToDecimal(xml.Elements("ValCurs").Elements("Valute")
                        .FirstOrDefault(x => x.Element("NumCode").Value == "398")
                        .Elements("Value").FirstOrDefault().Value)
                    };

                    // We use RAM on the server to have constant access to data.
                    _memoryCache.Set("key_currency", converter, TimeSpan.FromMinutes(1440));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex.InnerException.Message);
                }

                // if the specified task has not been completed, we request data updates every hour.
                // 1 hour = 60 minutes * 60 seconds * 1000 ms = 3,600,000 ms
                await Task.Delay(3600000, stoppingToken);
            }
        }
    }
}
