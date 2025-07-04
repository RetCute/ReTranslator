using OpenAI;
using OpenAI.Chat;
using ReTranslator.Utilities;
using System.ClientModel;

namespace ReTranslator.Translators;

public class GPTAPI: TranslatorBase
{
    private string api_key;
    private string api_base;
    private string model;
    private ChatClient Client;
    private List<ChatMessage> Messages = new();

    public override void Init(ConfigManager.TranslatorConfig config)
    {
        Logger.Info("初始化ChatGPT API中...");
        api_key = config.GetParameter<string>("api_key");
        api_base = config.GetParameter<string>("api_base");
        model = config.GetParameter<string>("model");
        if (!string.IsNullOrEmpty(api_key))
        {
            Client = new(
                model: model,
                credential: new ApiKeyCredential(api_key),
                options: new OpenAIClientOptions()
                {
                    Endpoint = new Uri(api_base)
                }
            );
            Messages.Add(new SystemChatMessage(
                "请将下面这段日文符合语气地优美地贴合原意地翻译为中文并且结合我消息记录的前几个句子进行翻译只给出翻译后的结果即可无需添加其他东西"
            ));
            Running = true;
            Logger.Success("ChatGPT API初始化成功");
        }
        else
        {
            Logger.Error("初始化ChatGPT API失败：apikey不能为空");
        }
    }

    private void TrimUserMessages(int maxUserMessages)
    {
        // 保留 SystemChatMessage
        var system = Messages.OfType<SystemChatMessage>().FirstOrDefault();
        
        var recentUserMessages = Messages
            .OfType<UserChatMessage>()
            .TakeLast(maxUserMessages)
            .ToList();

        Messages = new List<ChatMessage>();
        if (system != null)
            Messages.Add(system);
        Messages.AddRange(recentUserMessages);
    }
    public override string Translate(string text)
    {
        try
        {
            Messages.Add(new UserChatMessage(text));
            ChatCompletion completion =  Client.CompleteChatAsync(Messages).Result;
            TrimUserMessages(5);
            return completion.Content[0].Text;
        }
        catch (Exception ex)
        {
            Logger.Error(ex.ToString());
            return "Error";
        }
    }

    public override void Quit()
    {
        Running = false;
    }
}