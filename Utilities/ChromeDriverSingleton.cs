using SeleniumUndetectedChromeDriver;
using OpenQA.Selenium.Chrome;

namespace ReTranslator.Utilities
{
    public sealed class ChromeDriverSingleton : IDisposable
    {
        private static readonly Lazy<ChromeDriverSingleton> _instance = new(() => new ChromeDriverSingleton());
        public static ChromeDriverSingleton Instance => _instance.Value;

        public UndetectedChromeDriver Driver { get; private set; }

        private ChromeDriverSingleton() { }

        public void Init(string driverPath, string chromePath)
        {
            if (Driver != null) return; // 已经初始化过

            var options = new ChromeOptions();
            options.BinaryLocation = chromePath;
            options.AddArgument("--disable-blink-features=AutomationControlled");
            options.AddArgument("--no-sandbox");
            options.AddArgument("--disable-gpu");
            options.AddArgument("--disable-dev-shm-usage");
            options.AddArgument("--start-maximized");

            Driver = UndetectedChromeDriver.Create(driverExecutablePath: driverPath, options: options, hideCommandPromptWindow: true);
            Driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromSeconds(10);
        }

        public void Quit()
        {
            if (Driver != null)
            {
                Driver.Quit();
                Driver.Dispose();
                Driver = null;
            }
        }

        public void Dispose()
        {
            Quit();
        }
    }
}