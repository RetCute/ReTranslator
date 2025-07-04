using OpenQA.Selenium;
using SeleniumUndetectedChromeDriver;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium.Support.UI;
using ReTranslator.Utilities;

namespace ReTranslator.Translators
{
    public class WebTranslatorThread
    {
        private readonly GPTWeb _translator;
        private Thread _thread;

        public WebTranslatorThread(GPTWeb translator)
        {
            _translator = translator;
        }

        public void Start()
        {
            _thread = new Thread(Run);
            _thread.IsBackground = true;
            _thread.Start();
        }

        private void Run()
        {
            try
            {
                Logger.Info("初始化ChatGPT Web中...");
                
                ChromeDriverSingleton.Instance.Init(_translator.driver_path, _translator.chrome_path);
                // 打开 ChatGPT
                var driver = ChromeDriverSingleton.Instance.Driver;
                driver.Navigate().GoToUrl("https://chatgpt.com/auth/login");
                var wait = new WebDriverWait(driver, TimeSpan.FromSeconds(100));
                wait.Until(d  => d.Url.Contains("https://auth.openai.com/"));
                Logger.Info("等待登陆中...");
                // 等待对话界面加载完成
                wait.Until(d => d.FindElement(By.ClassName("group")));
                
                // 发送初始化消息
                var textBox = driver.FindElement(By.Id("prompt-textarea"));
                textBox.Clear();
                textBox.SendKeys("请将下列我给出的日文符合语气地优美地贴合原意地并结合我消息记录的前几个句子翻译为中文只给出翻译后的结果即可无需添加其他东西，「」所包裹的是要被翻译的内容,括号前面或者冒号前可能为人名结合语境不要翻译成其他的,如果表达的为人说的话记得加:");
                
                var sendButton =driver.FindElement(By.CssSelector("[data-testid='send-button']"));
                sendButton.Click();
                
                Logger.Success("ChatGPT Web初始化成功");
                _translator.Running = true;
            }
            catch (Exception ex)
            {
                Logger.Error($"Translator初始化失败: {ex}");
                _translator.Running = false;
            }
        }
    }

    public class GPTWeb: TranslatorBase
    {
        public string driver_path;
        public string chrome_path;

        public override void Init(ConfigManager.TranslatorConfig config)
        {
            driver_path = config.GetParameter<string>("driver_path");
            chrome_path = config.GetParameter<string>("chrome_path");
            if (!System.IO.File.Exists(driver_path) && !System.IO.File.Exists(chrome_path))
            {
                Logger.Error("Chrome和Chrome驱动不存在设定目录下,请在设置中调整");
            }
            else
            {
                var initThread = new WebTranslatorThread(this);
                initThread.Start();
            }
        }
        

        public override string Translate(string text)
        {
            Ask(text);
            return GetLastReply();
        }

        public override void Quit()
        {
            ChromeDriverSingleton.Instance.Quit();
        }

        private void Ask(string text)
        {
            try
            {
                var Driver = ChromeDriverSingleton.Instance.Driver;
                var wait = new WebDriverWait(Driver, TimeSpan.FromSeconds(5));
                wait.Until(d => d.FindElement(By.Id("prompt-textarea")).Enabled);
                
                var textBox = Driver.FindElement(By.Id("prompt-textarea"));
                textBox.Clear();
                textBox.SendKeys(text);
                
                var sendButton = Driver.FindElement(By.CssSelector("[data-testid='send-button']"));
                sendButton.Click();
            }
            catch (Exception ex)
            {
                Logger.Warning($"发送消息失败: {ex}");
            }
        }

        private string GetLastReply(double timeout = 20)
        {
            double elapsed = 0;
            const double interval = 0.5;
            var Driver = ChromeDriverSingleton.Instance.Driver;
            try
            {
                string previous = "";
                string current = "";

                while (elapsed < timeout)
                {
                    var replyElements = Driver.FindElements(By.CssSelector(".markdown"));

                    if (replyElements.Count > 0)
                    {
                        current = replyElements[^1].Text;

                        if (current == previous && !string.IsNullOrEmpty(current))
                        {
                            // 文本未变 且非空，说明生成结束
                            break;
                        }

                        previous = current;
                    }

                    Thread.Sleep(TimeSpan.FromSeconds(interval));
                    elapsed += interval;
                }

                if (elapsed >= timeout)
                {
                    Logger.Warning("获取回复超时");
                    return "Timeout Error!";
                }

                return current;
            }
            catch (Exception ex)
            {
                Logger.Error($"获取回复失败: {ex}");
                return "Error retrieving reply";
            }
        }
    }
}