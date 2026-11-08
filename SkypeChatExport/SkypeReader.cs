using Microsoft.Data.Sqlite;
using SkypeChatExport.Helper;
using SkypeChatExport.Model;
using System.IO;

namespace SkypeChatExport
{
    public class SkypeReader
    {
        private readonly string databaseFile;

        private Dictionary<int, int> chatMessagesCount = [];

        public SkypeReader(string databaseFile)
        {
            this.databaseFile = databaseFile;
            ArgumentNullException.ThrowIfNullOrWhiteSpace(databaseFile);

            if (!System.IO.File.Exists(databaseFile))
                throw new FileNotFoundException("database file not found!");
        }

        public async Task<IEnumerable<Account>> GetAccountsAsync()
        {
            List<Account> accounts = [];

            using (var connection = new SqliteConnection($"Data Source={databaseFile}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"SELECT id, skypename, fullname, birthday, city, emails, id FROM Accounts";

                // Type = 1 seem to be normal people!
                command.Parameters.AddWithValue("$type", 1);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var id = await GetStringValueAsync(reader, 0);
                        var skypeName = await GetStringValueAsync(reader, 1);
                        var fullName = await GetStringValueAsync(reader, 2);
                        var birthday = Helper.DateTimeHelper.ParseSkypeBirthday(reader.GetString(3))?.Date;
                        var city = await GetStringValueAsync(reader, 4);
                        var emails = await GetStringValueAsync(reader, 5);

                        accounts.Add(new Account() { ID = id, SkypeName = skypeName, FullName = fullName, Birthday = birthday, City = city, Emails = emails });
                    }
                }
            }


            return accounts;
        }

        public async Task<IEnumerable<Conversation>> ReadConversationsAsync()
        {
            List<Conversation> result = [];

            using (var connection = new SqliteConnection($"Data Source={databaseFile}"))
            {
                connection.Open();

                var command = connection.CreateCommand();
                command.CommandText = @"SELECT displayname, identity, id FROM Conversations WHERE type = $type";

                // Type = 1 seem to be normal people!
                command.Parameters.AddWithValue("$type", 1);

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var displayName = await GetStringValueAsync(reader, 0);
                        var identity = await GetStringValueAsync(reader, 1);
                        var id = int.Parse(await GetStringValueAsync(reader, 2));

                        result.Add(new Conversation(id, identity, displayName));
                    }
                }
            }

            return result;
        }        

        public async Task<ChatResult> LoadChatAsync(Conversation conversation, string? skypeName, int page = 1, string? search = null)
        {
            ChatResult result = new ChatResult();

            ArgumentNullException.ThrowIfNull(conversation);

            using (var connection = new SqliteConnection($"Data Source={databaseFile}"))
            {
                connection.Open();           

                // Check if need to load total message count (for paging)
                if (chatMessagesCount.TryGetValue(conversation.Id, out int count)) 
                {
                    result.TotalMessages = count;
                }
                else
                {
                    var cmd = connection.CreateCommand();
                    cmd.CommandText = @"SELECT COUNT(timestamp) FROM Messages WHERE param_value is NULL AND convo_id = $id";
                    cmd.Parameters.AddWithValue("$id", conversation.Id);

                    var msgCount = await cmd.ExecuteScalarAsync();
                    int value = int.Parse(msgCount.ToString());
                    chatMessagesCount.Add(conversation.Id, value);

                    result.TotalMessages = value;  
                }

                var command = connection.CreateCommand();

                int paging = 0;
                if (result.TotalPageCount > 1)
                {
                    paging = (page - 1) * Consts.PAGE_ENTRIES;
                    result.CurrentPage = page;
                }

                // Build SQL Query (SQL Lite)
                if (string.IsNullOrEmpty(search))
                {
                    command.CommandText = @"SELECT timestamp, body_xml, author, from_dispname, chatname, sending_status FROM Messages WHERE param_value is NULL AND convo_id = $id LIMIT $limit OFFSET $offset";
                    command.Parameters.AddWithValue("$id", conversation.Id);
                    command.Parameters.AddWithValue("$limit", Consts.PAGE_ENTRIES);
                    command.Parameters.AddWithValue("$offset", paging);
                }
                else
                {
                    command.CommandText = @"SELECT timestamp, body_xml, author, from_dispname, chatname, sending_status FROM Messages WHERE param_value is NULL AND convo_id = $id AND body_xml LIKE $search LIMIT $limit OFFSET $offset";
                    command.Parameters.AddWithValue("$id", conversation.Id);
                    command.Parameters.AddWithValue("$search", $"%{search}%");
                    command.Parameters.AddWithValue("$limit", Consts.PAGE_ENTRIES);
                    command.Parameters.AddWithValue("$offset", paging);
                }

                using (var reader = await command.ExecuteReaderAsync())
                {
                    while (await reader.ReadAsync())
                    {
                        var timestamp = DateTimeHelper.ConvertUnixTimestampToDateTime(double.Parse(reader.GetString(0)));

                        string? body = await GetStringValueAsync(reader, 1); 
                        var author = await GetStringValueAsync(reader, 2);
                        var from_dispname = await GetStringValueAsync(reader, 3);
                        var chatname = await GetStringValueAsync(reader, 4);
                        var sending_state = await GetStringValueAsync(reader, 5);

                        bool isOwnChat = author == skypeName;
                        if (string.IsNullOrEmpty(skypeName))
                            isOwnChat = (sending_state != null);

                        result.Messages.Add(new ChatMessage(body ?? string.Empty)
                        {
                            DateTime = timestamp,
                            Author = from_dispname ?? author ?? string.Empty,
                            IsOwnChat = isOwnChat,
                        });
                    }
                }
            }

            return result;
        }

        private static async Task<string?> GetStringValueAsync(SqliteDataReader reader, int index)
        {
            if (await reader.IsDBNullAsync(index))
                return null;

            return reader.GetString(index);
        }
    }
}