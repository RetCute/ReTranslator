using ReTranslator.Utilities;

namespace ReTranslator.Translators;

public abstract class TranslatorBase
{
    public bool Running { get; set; }

    public abstract void Init(ConfigManager.TranslatorConfig config);
    public abstract string Translate(string text);
    
    public abstract void Quit();
}