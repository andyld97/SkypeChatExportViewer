namespace SkypeChatExport.Model
{
    public class Account
    {
        public string ID { get; set; } = string.Empty;

        public string SkypeName { get; set; } = string.Empty;

        public string FullName { get; set; } = string.Empty;

        public DateTime? Birthday { get; set; } = null;

        public string City { get; set; } = string.Empty;

        public string Emails { get; set; } = string.Empty;

        public override string ToString()
        {
            return $"SkypeName: {SkypeName}, FullName: {FullName}, Birthday: {Birthday?.ToShortDateString()}, City: {City}, Emails: {Emails}";
        }
    }
}
