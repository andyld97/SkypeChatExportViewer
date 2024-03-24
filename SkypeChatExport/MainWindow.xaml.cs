using Microsoft.Win32;
using SkypeChatExport.Controls;
using SkypeChatExport.Controls.Dialogs;
using SkypeChatExport.Model;
using System.Windows;

namespace SkypeChatExport
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly bool isInitalized = false;
        private bool isSkypeDatabaseLoaded = false;

        private SkypeReader skypeReader;
        
        public MainWindow()
        {
            InitializeComponent();

            Loaded += MainWindow_Loaded;
            isInitalized = true;

            SetState(false);
        }

        #region GUI Methods

        public async Task InitializeAsync(string path)
        {
            try
            {
                skypeReader = new SkypeReader(path);
                Conversations.ItemsSource = await ReadConversationsAsync();
                SetState(true);

                MessageBox.Show("The database was opened successfully! You can now select the conversation to view the chat!", "Success!", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to open Skype database: {ex.Message}", "Critical Error!", MessageBoxButton.OK, MessageBoxImage.Error);
                SetState(false);
            }
        }

        private void SetState(bool success)
        {
            // Enable/Disable everything
            if (!success)
            {
                Conversations.IsEnabled = false;
                TextSearch.IsEnabled = false;
                isSkypeDatabaseLoaded = false;
                Conversations.ItemsSource = null;
                ButtonClearSearch.IsEnabled = false;
            }
            else
            {
                Conversations.IsEnabled = true;
                TextSearch.IsEnabled = true;
                isSkypeDatabaseLoaded = true;
                ButtonClearSearch.IsEnabled = true;
            }
        }

        #endregion

        #region Skype Reader Wrapper Methods

        private async Task<IEnumerable<Conversation>> ReadConversationsAsync()
        {
            List<Conversation> result = [];

            try
            {
                var conversations = await skypeReader.ReadConversationsAsync();
                result = conversations.ToList();
                SetState(true);
            }
            catch (Exception ex)
            {
                SetState(false);
                MessageBox.Show($"Failed to read data: {ex.Message}!", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return result;
        }

        private async Task<IEnumerable<ChatMessage>?> LoadChatAsync(Conversation conversation, string? search = null)
        {
            IEnumerable<ChatMessage>? messages = null;

            try
            {
                var result = await skypeReader.LoadChatAsync(conversation, currentPage, search);
                messages = result.Messages;

                if (maxPages == null)
                    maxPages = result.TotalPageCount;

                SetState(true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Failed to read data: {ex.Message}!", "Critical Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }

            return messages;
        }

        #endregion

        #region Render Chats

        private void RenderChats(IEnumerable<ChatMessage>? messages, bool clearPreviousMessages)
        {
            if (clearPreviousMessages)
                Messages.Children.Clear();

            if (messages == null || !messages.Any())
            {
                if (!string.IsNullOrEmpty(TextSearch.Text))
                    TextNoMessagesFound.Visibility = Visibility.Visible;

                return;
            }
            else
                TextNoMessagesFound.Visibility = Visibility.Hidden;

            ChatMessage? lastMessage = null;
            Message? lastMessageControl = null;

            foreach (var item in messages)
            {
                Message message = new Message(item)
                {
                    Margin = new Thickness(10),
                    Width = 450,
                    HorizontalAlignment = item.IsOwnChat ? HorizontalAlignment.Right : HorizontalAlignment.Left
                };

                if (lastMessage != null)
                {
                    if (item.IsOwnChat == lastMessage.IsOwnChat && item.Author == lastMessage.Author)
                    {
                        if ((item.DateTime.Subtract(lastMessage.DateTime)) < TimeSpan.FromSeconds(10))
                        {
                            lastMessageControl!.Margin = new Thickness(10, 5, 10, 10);
                        }
                    }
                }

                Messages.Children.Add(message);

                lastMessage = item;
                lastMessageControl = message;
            }
        }
        #endregion

        #region Paging
        private Conversation currentConverstation;
        private int currentPage = 1;
        private int? maxPages = null;
        #endregion

        #region Paging Events

        private async void MainScrollviewer_ScrollChanged(object sender, System.Windows.Controls.ScrollChangedEventArgs e)
        {
            if (!isInitalized || currentConverstation == null)
                return;

            if (MainScrollviewer.VerticalOffset == MainScrollviewer.ScrollableHeight)
            {
                // Load more items
                currentPage++;

                if (currentPage > maxPages)
                    return;

                var messages = await LoadChatAsync(currentConverstation, TextSearch.Text);
                RenderChats(messages, false);
            }
        }

        #endregion

        #region Events
        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            // Load main db or file arguments

            // Check if there is an argument
            string[] args = Environment.GetCommandLineArgs();
            if (args.Length >= 2)
            {
                if (args[1].EndsWith(".db"))
                {
                    await InitializeAsync(args[1]);
                    return;
                }
            }

            // Check if there is a main.db file in this directory
            try
            {
                string? dir = System.IO.Path.GetDirectoryName(System.Diagnostics.Process.GetCurrentProcess().MainModule.FileName);
                var di = new System.IO.DirectoryInfo(dir);
                var files = di.EnumerateFiles("*.db");
                var file = files.FirstOrDefault(f => f.Name == "main.db");
                if (file == null) return;

                await InitializeAsync(file.FullName);
                return;
            }
            catch
            {
                // ignore
            }
        }

        private async void Conversations_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            if (!isInitalized)
                return;

            if (Conversations.SelectedItem is Conversation conversation)
            {
                // Reset paging
                currentConverstation = conversation;
                currentPage = 1;
                maxPages = null;

                var messages = await LoadChatAsync(conversation);
                RenderChats(messages, true);
            }
        }

        private async void TextSearch_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e)
        {
            if (currentConverstation == null)
                return;

            var messages = await LoadChatAsync(currentConverstation, TextSearch.Text);
            RenderChats(messages, true);
        }

        private void ButtonClearSearch_Click(object sender, RoutedEventArgs e)
        {
            TextSearch.Clear();
        }
        
        #region Menu      

        private async void MenuOpenFile_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog ofd = new OpenFileDialog();
            ofd.Filter = "Skype Main Database|*.db;";

            var result = ofd.ShowDialog();
            if (result == true)
            {
                SetState(false);
                await InitializeAsync(ofd.FileName);
            }
        }

        private void MenuAbout_Click(object sender, RoutedEventArgs e)
        {
            new AboutDialog().ShowDialog();
        }

        private void ButtonExit_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        #endregion

        #endregion
    }
}