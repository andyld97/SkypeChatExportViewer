namespace SkypeChatExport.Model
{
    public class Conversation
    {
        public int Id { get; set; }

        public string? Identity { get; set; }

        public string? DisplayName { get; set; }    

        public Conversation()
        {

        }

        public Conversation(int id, string? identity, string? displayName)
        {
            Id = id;
            Identity = identity;
            DisplayName = displayName;
        }

        public override string ToString()
        {
            return Identity;
        }
    }
}
