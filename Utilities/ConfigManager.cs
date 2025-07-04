using System;
using System.IO;
using System.Runtime.CompilerServices;
using YamlDotNet.Serialization;
using YamlDotNet.Serialization.NamingConventions;
using Newtonsoft.Json;

namespace ReTranslator.Utilities;

public class ConfigManager
{
    private static readonly Lazy<ConfigManager> _instance = 
        new Lazy<ConfigManager>(() => new ConfigManager());
    public static ConfigManager Instance => _instance.Value;
    
    private const string ConfigFilePath = "config.yml";
    private const string TranslatorsFilePath = "translators.json";
    private const string DefaultConfig = @"size: 20
TextARGB: ""(255,255,0,0)""
captureHotkey: ""w""
pauseHotkey: ""q""
Text_Extraction_Mode: 0
TextractorPath: """"
TranslationMethod: 0
Translators:
  - name: ""ChatGPT Web""
    parameters:
      driver_path: """"
      chrome_path: """"
      
  - name: ""ChatGPT API""
    parameters:
      api_key: """"
      api_base: ""https://api.openai.com/v1""
      model: ""gpt3.5-turbo""";

    public AppConfig Config { get; private set; } = new AppConfig();
    public event EventHandler ConfigChanged;
    private ConfigManager()
    {
        LoadConfig();
    }
    
    public void LoadConfig()
    {
        Logger.Info("读取配置文件中...");
        while (true)
        {
            try
            {
                using var reader = new StreamReader(ConfigFilePath);
                var deserializer = new DeserializerBuilder()
                    .WithNamingConvention(UnderscoredNamingConvention.Instance) // 匹配下划线命名
                    .IgnoreUnmatchedProperties() // 忽略不匹配的属性
                    .Build();

                Config = deserializer.Deserialize<AppConfig>(reader);
                Logger.Success("配置文件加载成功");
                break;
            }
            catch (Exception ex)
            {
                Logger.Error($"加载配置文件失败: {ex}");
                Logger.Warning("使用默认配置...");
                CreateDefaultConfig();
            }
        }
    }
    
    public void SaveConfig(AppConfig config)
    {
        try
        {
            var serializer = new SerializerBuilder()
                .WithNamingConvention(UnderscoredNamingConvention.Instance)
                .Build();
                
            var yaml = serializer.Serialize(config);
                
            File.WriteAllText(ConfigFilePath, yaml);
            Config = config;
            ConfigChanged?.Invoke(this, EventArgs.Empty);
            Logger.Info("配置文件保存成功");
        }
        catch (Exception ex)
        {
            Logger.Error($"保存配置文件失败: {ex}");
        }
    }
    
    private static void CreateDefaultConfig()
    {
        File.WriteAllText(ConfigFilePath, DefaultConfig);
    }
    
    public class AppConfig
    {
        [YamlMember(Alias = "size", ApplyNamingConventions = false)]
        public int Size { get; set; } = 20;

        [YamlMember(Alias = "TextARGB", ApplyNamingConventions = false)]
        public string TextARGB { get; set; } = "(255,0,0,255);";

        [YamlMember(Alias = "captureHotkey", ApplyNamingConventions = false)]
        public string CaptureHotkey { get; set; } = "w";

        [YamlMember(Alias = "pauseHotkey", ApplyNamingConventions = false)]
        public string PauseHotkey { get; set; } = "q";

        [YamlMember(Alias = "Text_Extraction_Mode", ApplyNamingConventions = false)]
        public int TextExtractionMode { get; set; } = 0;

        [YamlMember(Alias = "TextractorPath", ApplyNamingConventions = false)]
        public string TextractorPath { get; set; } = "";

        [YamlMember(Alias = "TranslationMethod", ApplyNamingConventions = false)]
        public int TranslationMethod { get; set; } = 0;
        
        [YamlMember(Alias = "Translators", ApplyNamingConventions = false)]
        public List<TranslatorConfig> Translators { get; set; } = new List<TranslatorConfig>
        {
            new TranslatorConfig
            {
                name = "ChatGPT Web",
                parameters = new Dictionary<string, object>
                {
                    { "driver_path", "" },
                    { "chrome_path", "" }
                }
            },
            new TranslatorConfig
            {
                name = "ChatGPT API",
                parameters = new Dictionary<string, object>
                {
                    { "api_key", "" },
                    { "api_base", "" },
                    { "model", ""}
                }
            }
        };
    }

    public class TranslatorConfig
    {
        public string name { get; set; } = "";
        public Dictionary<string, object> parameters { get; set; } = new Dictionary<string, object>();

        public T GetParameter<T>(string key, T defaultValue = default)
        {
            if (parameters.TryGetValue(key, out object value))
            {
                try
                {
                    return (T)Convert.ChangeType(value, typeof(T));
                }
                catch
                {
                    return defaultValue;
                }
            }

            return defaultValue;
        }

        // 设置参数值
        public void SetParameter(string key, object value)
        {
            parameters[key] = value;
        }
    }
}