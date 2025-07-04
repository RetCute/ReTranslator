using System.Windows;
using System.Windows.Controls;
using ReTranslator.Translators;

namespace ReTranslator.Utilities
{
    public partial class HookCodePanel : UserControl
    {
        private readonly Dictionary<string, List<Dictionary<string, string>>> messages = new();
        private TranslatorBase Translator;
        private SubtitleWindow Subtitle;

        public HookCodePanel()
        {
            InitializeComponent();
        }

        public void setTranslatorAndSubtitle(TranslatorBase translator, SubtitleWindow subtitle)
        {
            Translator = translator;
            Subtitle = subtitle;
        }
        
        public void ProcessText(string text)
        {
            if (text.StartsWith("[") && text.Contains("]"))
            {
                int endIndex = text.IndexOf("]");
                string key = text.Substring(1, endIndex - 1);
                string value = text.Substring(endIndex + 1).Trim();

                if (!messages.ContainsKey(key))
                {
                    messages[key] = new List<Dictionary<string, string>>();
                    ComboBox.Items.Add(key);
                }

                string reply = "未翻译";

                if (ComboBox.SelectedItem?.ToString() == key && !key.Contains("控制台") && !key.Contains("剪贴"))
                {
                    reply = Translator.Translate(value);
                    Subtitle.UpdateSubtitle(reply);
                }

                messages[key].Add(new Dictionary<string, string> { { value, reply } });

                if (ComboBox.SelectedItem?.ToString() == key)
                {
                    AddMessageWidget(key, value, reply);
                }
            }
        }

        private void AddMessageWidget(string key, string text1, string text2)
        {
            var widget = new MessageWidget(text1, text2, key);
            widget.UpdateBtn.Click += (s, e) => UpdateSubtitle(widget);
            ScrollPanel.Children.Add(widget);
            scrollViewer.ScrollToBottom();
        }

        private void ComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ScrollPanel.Children.Clear();
            string selectedKey = ComboBox.SelectedItem?.ToString();
            if (selectedKey == null) return;

            if (messages.TryGetValue(selectedKey, out var list))
            {
                foreach (var dict in list)
                {
                    foreach (var kv in dict)
                    {
                        AddMessageWidget(selectedKey, kv.Key, kv.Value);
                    }
                }
            }
        }

        private void UpdateSubtitle(MessageWidget widget)
        {
            if (!widget.Text2.Contains("未翻译") && !widget.Text2.Contains("Error"))
            {
                Subtitle.UpdateSubtitle(widget.Text2);
            }
            else
            {
                string original = widget.Text1;
                string translated = Translator.Translate(original);
                widget.Text2 = translated;
                Subtitle.UpdateSubtitle(translated);

                foreach (var dict in messages[widget.Key])
                {
                    if (dict.ContainsKey(original))
                        dict[original] = translated;
                }
            }
        }
    }
}
