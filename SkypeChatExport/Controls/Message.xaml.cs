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
        public Message(ChatMessage message)
        {
            InitializeComponent();
            Update(message);
        }

        public void Update(ChatMessage message)
        {
            string msg = message.Message;

            // Replace <ss type="smile">:)</ss> (but tags only) 
            msg = StripXMLTagsRegex().Replace(msg, string.Empty);

            TextMessage.Text = msg;
            TextName.Text = message.Author;
            TextDate.Text = message.DateTime.ToString("dd.MM.yyyy HH:mm");
        }

        [GeneratedRegex("<.*?>")]
        private static partial Regex StripXMLTagsRegex();
    }
}