using System.Windows.Controls;

namespace ReTranslator.Utilities
{
    public partial class MessageWidget : UserControl
    {
        public string Key { get; }

        public string Text1
        {
            get => Label1.Text.Replace("[原文]", "");
            set => Label1.Text = "[原文]" + value;
        }

        public string Text2
        {
            get => Label2.Text.Replace("[译文]", "");
            set => Label2.Text = "[译文]" + value;
        }

        public Button UpdateBtn => UpdateButton;

        public MessageWidget(string text1, string text2, string key)
        {
            InitializeComponent();
            Key = key;
            Text1 = text1;
            Text2 = text2;
        }
    }
}