using SkypeChatExport.Model;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace SkypeChatExport.Controls
{
    /// <summary>
    /// Interaktionslogik für Message.xaml
    /// </summary>
    public partial class Message : UserControl
    {
        private ChatMessage message;

        public Message(ChatMessage message)
        {
            InitializeComponent();
            this.message = message;
            Update(message);
        }

        public void Update(ChatMessage message)
        {
            this.message = message;
            string msg = message.Message;

            // Replace <ss type="smile">:)</ss> (but tags only) 
            msg = StripXMLTagsRegex().Replace(msg, string.Empty);

            TextMessage.Text = msg;
            TextName.Text = message.Author;
            TextDate.Text = message.DateTime.ToString("g");
        }

        [GeneratedRegex("<.*?>")]
        private static partial Regex StripXMLTagsRegex();

        private void Border_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
                System.Windows.Clipboard.SetText(message.Message);
        }
    }
}