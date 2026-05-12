using System.Windows;
namespace X6ProUnLocker
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            // Force English by default, toggle available in UI
            Core.LanguageManager.SetLanguage("en");
            base.OnStartup(e);
        }
    }
}