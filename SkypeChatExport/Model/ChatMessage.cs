namespace SkypeChatExport.Model
{
    public class ChatMessage
    {
        public string Message { get; set; }

        public DateTime DateTime { get; set; }

        public string Author { get; set; }

        public bool IsOwnChat { get; set; }

        public ChatMessage(string message)
        {
            Message = message;
        }

        public override string ToString()
        {
            return Message;
        }
    }

    public class ChatResult
    {
        public int MessagesCount { get; set; }

        public int TotalMessages { get; set; }

        public int TotalPageCount
        {
            get
            {
                double result = TotalMessages / (double)Consts.PAGE_ENTRIES;
                if (result < 1)
                    return 1;

                if (Math.Floor(result) == result)
                    return (int)result;

                return (int)(Math.Floor(result) + 1);
            }
        }

        public int CurrentPage {  get; set; }

        public List<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}