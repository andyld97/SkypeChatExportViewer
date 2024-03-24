using System.Windows;

namespace SkypeChatExport.Controls.Dialogs
{
    /// <summary>
    /// Interaktionslogik für AboutDialog.xaml
    /// </summary>
    public partial class AboutDialog : Window
    {
        public AboutDialog()
        {
            InitializeComponent();
        }

        private void Hyperlink_RequestNavigate(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            OpenUri(new Uri("https://icons8.com/"));
        }


        private void Hyperlink_RequestNavigate_1(object sender, System.Windows.Navigation.RequestNavigateEventArgs e)
        {
            OpenUri(new Uri("https://github.com/Tyrrrz/DotnetRuntimeBootstrapper"));
        }

        /// <summary>
        /// Opens the default system browser with the requested uri
        /// https://stackoverflow.com/questions/4580263/how-to-open-in-default-browser-in-c-sharp/67838115#67838115
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        public static bool OpenUri(Uri uri)
        {
            try
            {
                System.Diagnostics.Process.Start("explorer.exe", $"\"{uri}\"");
                return true;
            }
            catch (Exception)
            {
                // ignore 
                return false;
            }
        }
    }
}
