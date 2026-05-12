using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Principal;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;
using X6ProUnLocker.Core;

namespace X6ProUnLocker
{
    public partial class MainWindow : Window
    {
        private bool isAdmin = false;
        private string selectedExePath = "";

        public MainWindow()
        {
            InitializeComponent();
            CheckEnvironment();
            RefreshProcesses();
            Log("=== X6ProUnLocker v1.1.0 — System Reanimator ===", Colors.Gold);
            Log("🔥 POCO Systems - Powerful recovery tools", Colors.Gold);
            Log($"Start time: {DateTime.Now:dd.MM.yyyy HH:mm:ss}");
        }

        private void CheckEnvironment()
        {
            isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent()).IsInRole(WindowsBuiltInRole.Administrator);
            bool isSafe = Environment.GetCommandLineArgs().Any(a => a.Equals("/safeboot", StringComparison.OrdinalIgnoreCase));
            bool isWinPE = Directory.Exists(@"X:\Windows") || Environment.GetFolderPath(Environment.SpecialFolder.System).Contains("WinPE", StringComparison.OrdinalIgnoreCase);
            bool isWinRE = Directory.Exists(@"X:\Recovery") || Environment.GetFolderPath(Environment.SpecialFolder.System).Contains("WinRE", StringComparison.OrdinalIgnoreCase);

            string envText, envColor;
            if (isWinPE) { envText = LanguageManager.Get("EnvWinPE"); envColor = "#FF5722"; }
            else if (isWinRE) { envText = LanguageManager.Get("EnvWinRE"); envColor = "#FF9800"; }
            else if (isSafe) { envText = LanguageManager.Get("EnvSafe"); envColor = "#FFC100"; }
            else { envText = LanguageManager.Get("EnvNormal"); envColor = "#4CAF50"; }

            if (!isAdmin) envText += LanguageManager.Get("NoAdmin");

            EnvText.Text = envText;
            EnvBorder.Background = new SolidColorBrush((Color)ColorConverter.ConvertFromString(envColor));
            Log($"✅ Environment detected: {envText}", Colors.LightGreen);
        }

        private void RefreshProcesses()
        {
            var list = ProcessManager.GetProcessList();
            ProcessGrid.ItemsSource = list.Select(p => new { p.Pid, p.Name, p.Path, MemoryMB = p.Memory / (1024.0 * 1024.0), CpuPercent = p.CpuPercent }).ToList();
            Log($"✅ Process list refreshed: {list.Count} processes", Colors.LightGreen);
        }

        private void RefreshProcesses_Click(object sender, RoutedEventArgs e) => RefreshProcesses();
        private void EndProcess_Click(object sender, RoutedEventArgs e) => KillProcess(false);
        private void KillProcess_Click(object sender, RoutedEventArgs e) => KillProcess(true);

        private void KillProcess(bool force)
        {
            if (ProcessGrid.SelectedItem == null) return;
            var item = (dynamic)ProcessGrid.SelectedItem;
            try { var p = Process.GetProcessById(item.Pid); if (force) p.Kill(); else p.CloseMainWindow(); Log($"{(force ? "💥" : "⏹️")} Process {item.Pid} terminated", force ? Colors.Orange : Colors.LightGreen); }
            catch (Exception ex) { Log(LanguageManager.Get("StatusError").Replace("{0}", ex.Message), Colors.Red); }
        }

        private void ShowProperties_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessGrid.SelectedItem == null) return;
            var item = (dynamic)ProcessGrid.SelectedItem;
            if (!string.IsNullOrEmpty(item.Path) && File.Exists(item.Path)) WinApiNative.ShowFileProperties(item.Path);
            else Log(LanguageManager.Get("StatusWarning").Replace("{0}", "File not found"), Colors.Yellow);
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessGrid.SelectedItem == null) return;
            var item = (dynamic)ProcessGrid.SelectedItem;
            if (!string.IsNullOrEmpty(item.Path) && File.Exists(item.Path)) Process.Start("explorer.exe", $"/select,\"{item.Path}\"");
            else Log(LanguageManager.Get("StatusWarning").Replace("{0}", "File not found"), Colors.Yellow);
        }

        private void BrowseExe_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog { Filter = "Executable files (*.exe)|*.exe" };
            if (dialog.ShowDialog() == true)
            {
                selectedExePath = dialog.FileName;
                ExePathBox.Text = selectedExePath;
                Log($"🔍 Selected: {selectedExePath}", Colors.HotPink);
            }
        }

        private void ReplaceCmd_Click(object sender, RoutedEventArgs e) => ReplaceUtility("cmd.exe");
        private void ReplaceSethc_Click(object sender, RoutedEventArgs e) => ReplaceUtility("sethc.exe");
        private void ReplaceUtilman_Click(object sender, RoutedEventArgs e) => ReplaceUtility("utilman.exe");

        private void ReplaceUtility(string name)
        {
            if (string.IsNullOrEmpty(selectedExePath)) { MessageBox.Show("Select an .exe first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning); return; }
            string sys = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string path = Path.Combine(sys, name);
            if (!File.Exists(path)) { MessageBox.Show($"Missing: {path}", "Error", MessageBoxButton.OK, MessageBoxImage.Error); return; }
            if (MessageBox.Show($"Replace {name}?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                if (FileManager.ReplaceSystemUtility(path, selectedExePath, Log))
                    Log(LanguageManager.Get("StatusSuccess").Replace("{0}", $"{name} replaced"), Colors.LightGreen);
                else Log(LanguageManager.Get("StatusError").Replace("{0}", "Failed"), Colors.Red);
            }
        }

        private void RestoreOriginals_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Restore backups?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                string sys = Environment.GetFolderPath(Environment.SpecialFolder.System);
                SystemRestore.RestoreOriginalUtilities(sys, Log);
                Log(LanguageManager.Get("StatusSuccess").Replace("{0}", "Originals restored"), Colors.LightGreen);
            }
        }

        private void RestoreFonts_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Restore default fonts?", "Confirm", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                SystemRestore.RestoreSystemFonts();
                Log(LanguageManager.Get("StatusSuccess").Replace("{0}", "Fonts restored"), Colors.LightGreen);
            }
        }

        private void RefreshEnv_Click(object sender, RoutedEventArgs e) => CheckEnvironment();

        // Startup Tab
        private void RefreshStartup_Click(object sender, RoutedEventArgs e)
        {
            StartupGrid.ItemsSource = AutostartManager.GetEntries();
            Log("🔄 Startup entries refreshed", Colors.LightGreen);
        }
        private void AddStartupApp_Click(object sender, RoutedEventArgs e)
        {
            var d = new Microsoft.Win32.OpenFileDialog { Filter = "Executables|*.exe" };
            if (d.ShowDialog() == true)
            {
                if (AutostartManager.AddEntry(Path.GetFileNameWithoutExtension(d.FileName), d.FileName, "Registry"))
                    Log(LanguageManager.Get("StatusSuccess").Replace("{0}", "Added to startup"), Colors.LightGreen);
                else Log(LanguageManager.Get("StatusError").Replace("{0}", "Failed to add"), Colors.Red);
                RefreshStartup_Click(sender, e);
            }
        }
        private void RemoveStartupApp_Click(object sender, RoutedEventArgs e)
        {
            if (StartupGrid.SelectedItem == null) return;
            var item = (dynamic)StartupGrid.SelectedItem;
            if (AutostartManager.RemoveEntry(item.Name, item.Location))
                Log(LanguageManager.Get("StatusSuccess").Replace("{0}", "Removed from startup"), Colors.LightGreen);
            else Log(LanguageManager.Get("StatusError").Replace("{0}", "Failed to remove"), Colors.Red);
            RefreshStartup_Click(sender, e);
        }

        private void LangBtn_Click(object sender, RoutedEventArgs e)
        {
            string newLang = LanguageManager.Current == "en" ? "ru" : "en";
            LanguageManager.SetLanguage(newLang);
            LangBtn.Content = newLang == "en" ? "🌐 EN" : "🌐 RU";
            TitleText.Text = LanguageManager.Get("Title");
            SubtitleText.Text = LanguageManager.Get("Subtitle");
            CheckEnvironment();
        }

        private void Log(string message, Color color)
        {
            Dispatcher.Invoke(() =>
            {
                Paragraph para = new Paragraph();
                Run run = new Run($"{DateTime.Now:HH:mm:ss} {message}");
                run.Foreground = new SolidColorBrush(color);
                para.Inlines.Add(run);
                LogBox.Document.Blocks.Add(para);
                LogBox.ScrollToEnd();
                StatusLabel.Text = message;
            });
        }
        private void Log(string message) => Log(message, Colors.White);
    }
}