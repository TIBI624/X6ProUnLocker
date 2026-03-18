using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
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
        private bool isSafeMode = false;
        private bool isWinRE = false;
        private bool isWinPE = false;
        private string selectedExePath = "";
        private List<MalwareInfo> detectedMalware = new();
        private List<SystemIssue> detectedIssues = new();
        private List<AutoStartEntry> autoStartEntries = new();

        public MainWindow()
        {
            InitializeComponent();
            CheckEnvironment();
            RefreshProcesses();
            Log("=== X6ProUnLocker v1.0.0 — System Reanimator ===", Colors.Gold);
            Log("🔥 POCO Systems - Powerful recovery tools", Colors.Gold);
            Log("Start time: " + DateTime.Now.ToString("dd.MM.yyyy HH:mm:ss"));
            // Запуск ранней инициализации
            InitializeEarlyBoot();
        }

        private void CheckEnvironment()
        {
            isAdmin = new WindowsPrincipal(WindowsIdentity.GetCurrent())
                .IsInRole(WindowsBuiltInRole.Administrator);

            isSafeMode = WinApiNative.GetSystemMetrics(WinApiNative.SM_CLEANBOOT) != 0;

            // Определение WinRE/WinPE
            string systemDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
            isWinRE = systemDir.Contains("WinRE", StringComparison.OrdinalIgnoreCase) ||
                      systemDir.Contains("Recovery", StringComparison.OrdinalIgnoreCase);
            isWinPE = WinApiNative.GetSystemWindowsDirectory(null, 0) == 0; // если функция возвращает 0, значит не Windows

            string envText = "Environment: ";
            Color envColor = Colors.Green;

            if (isWinPE)
            {
                envText += "WinPE (Boot environment)";
                envColor = Colors.Red;
            }
            else if (isWinRE)
            {
                envText += "WinRE (Recovery environment)";
                envColor = Colors.Orange;
            }
            else if (isSafeMode)
            {
                envText += "Safe Mode";
                envColor = Colors.Gold;
            }
            else
            {
                envText += "Normal mode";
                envColor = Colors.Green;
            }

            if (!isAdmin)
            {
                envText += " | ⚠️ Without admin rights";
                envColor = Colors.OrangeRed;
            }

            EnvText.Text = envText;
            EnvBorder.Background = new SolidColorBrush(envColor);
            Log("✅ Environment detected: " + envText, Colors.LightGreen);
        }

        private void InitializeEarlyBoot()
        {
            Log("🔧 Initializing early boot...", Colors.CornflowerBlue);
            EarlyBootManager.RegisterEarlyBootService();
            EarlyBootManager.CheckCriticalSystemFiles(Log);
            Log("✅ Early boot initialization complete", Colors.LightGreen);
        }

        private void RefreshEnv_Click(object sender, RoutedEventArgs e)
        {
            CheckEnvironment();
        }

        public void RefreshProcesses()
        {
            var list = ProcessManager.GetProcessList();
            var items = new List<dynamic>();
            foreach (var p in list)
            {
                double cpuPercent = 0;
                string priority = "Normal";
                string company = "";
                try
                {
                    using var proc = Process.GetProcessById(p.Pid);
                    // CPU usage требует выборки дважды, для простоты оставим 0
                    priority = proc.PriorityClass.ToString();
                    company = FileVersionInfo.GetVersionInfo(proc.MainModule.FileName).CompanyName ?? "";
                }
                catch { }

                items.Add(new
                {
                    p.Pid,
                    p.Name,
                    p.Path,
                    MemoryMB = p.Memory / (1024.0 * 1024.0),
                    CpuPercent = cpuPercent,
                    Priority = priority,
                    Status = (p.Pid == 0 || p.Pid == 4) ? "System" : "Running",
                    Company = company
                });
            }
            ProcessGrid.ItemsSource = items;
            Log($"✅ Process list refreshed: {items.Count} processes", Colors.LightGreen);
        }

        private void RefreshProcesses_Click(object sender, RoutedEventArgs e)
        {
            RefreshProcesses();
        }

        private void EndProcess_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessGrid.SelectedItem == null) return;
            dynamic item = ProcessGrid.SelectedItem;
            int pid = item.Pid;
            try
            {
                var proc = Process.GetProcessById(pid);
                proc.CloseMainWindow();
                Log($"⏹️ Sent close request to process {pid} ({item.Name})");
            }
            catch (Exception ex)
            {
                Log($"❌ Failed to end process {pid}: {ex.Message}", Colors.Red);
            }
        }

        private void KillProcess_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessGrid.SelectedItem == null) return;
            dynamic item = ProcessGrid.SelectedItem;
            int pid = item.Pid;
            try
            {
                var proc = Process.GetProcessById(pid);
                proc.Kill();
                Log($"💥 Process {pid} ({item.Name}) killed", Colors.Orange);
            }
            catch (Exception ex)
            {
                Log($"❌ Failed to kill process {pid}: {ex.Message}", Colors.Red);
            }
        }

        private void ShowProperties_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessGrid.SelectedItem == null) return;
            dynamic item = ProcessGrid.SelectedItem;
            string path = item.Path;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                // Открыть окно свойств файла
                WinApiNative.ShowFileProperties(path);
            }
            else
            {
                Log("⚠️ Cannot show properties: file not found", Colors.Yellow);
            }
        }

        private void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            if (ProcessGrid.SelectedItem == null) return;
            dynamic item = ProcessGrid.SelectedItem;
            string path = item.Path;
            if (!string.IsNullOrEmpty(path) && File.Exists(path))
            {
                string folder = Path.GetDirectoryName(path);
                Process.Start("explorer.exe", folder);
            }
            else
            {
                Log("⚠️ Cannot open folder: file not found", Colors.Yellow);
            }
        }

        private void BrowseExe_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new Microsoft.Win32.OpenFileDialog
            {
                Filter = "Executable files (*.exe)|*.exe",
                InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.Desktop)
            };
            if (dialog.ShowDialog() == true)
            {
                selectedExePath = dialog.FileName;
                ExePathBox.Text = selectedExePath;
                Log($"🦠 Scanning file for viruses: {selectedExePath}", Colors.HotPink);
                // Асинхронно сканируем
                var scanner = new VirusScanner();
                scanner.ScanCompleted += (s, result) =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        if (result.IsMalware)
                            Log($"❌ {result.Message}", Colors.Red);
                        else
                            Log($"✅ {result.Message}", Colors.LightGreen);
                    });
                };
                scanner.ScanFileAsync(selectedExePath);
            }
        }

        private void ReplaceCmd_Click(object sender, RoutedEventArgs e) => ReplaceUtility("cmd.exe");
        private void ReplaceSethc_Click(object sender, RoutedEventArgs e) => ReplaceUtility("sethc.exe");
        private void ReplaceUtilman_Click(object sender, RoutedEventArgs e) => ReplaceUtility("utilman.exe");

        private void ReplaceUtility(string utilityName)
        {
            if (string.IsNullOrEmpty(selectedExePath))
            {
                MessageBox.Show("Please select an .exe file first.", "Error", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            string systemDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
            string utilityPath = Path.Combine(systemDir, utilityName);

            if (!File.Exists(utilityPath))
            {
                MessageBox.Show($"System file not found: {utilityPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                return;
            }

            var result = MessageBox.Show(
                $"Replace {utilityName} with {selectedExePath}?\nOriginal will be backed up as .bak",
                "Confirm replacement",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (result != MessageBoxResult.Yes) return;

            if (FileManager.ReplaceSystemUtility(utilityPath, selectedExePath, Log))
            {
                Log($"✅ {utilityName} successfully replaced with {selectedExePath}", Colors.LightGreen);
                MessageBox.Show($"{utilityName} replaced successfully!", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                Log($"❌ Failed to replace {utilityName}", Colors.Red);
                MessageBox.Show("Replacement failed.", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void RestoreOriginals_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Restore original utilities from .bak backups?",
                "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            string systemDir = Environment.GetFolderPath(Environment.SpecialFolder.System);
            SystemRestore.RestoreOriginalUtilities(systemDir, Log);
            Log("✅ Original utilities restored", Colors.LightGreen);
        }

        private void RestoreFonts_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                "This will restore standard Windows fonts.\nReboot recommended after completion.\nContinue?",
                "Restore fonts",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            Log("🔤 Restoring system fonts...", Colors.CornflowerBlue);
            SystemRestore.RestoreSystemFonts();
            Log("✅ Fonts restored (simulated)", Colors.LightGreen);
        }

        private void ApplyAutostart_Click(object sender, RoutedEventArgs e)
        {
            string appPath = Process.GetCurrentProcess().MainModule.FileName;
            int count = 0;
            if (RegRunCk.IsChecked == true)
            {
                AutostartManager.AddToRegistryCurrentUser(appPath);
                count++;
            }
            if (RegRunLMCk.IsChecked == true)
            {
                AutostartManager.AddToRegistryLocalMachine(appPath);
                count++;
            }
            if (StartupFolderCk.IsChecked == true)
            {
                AutostartManager.AddToStartupFolder(appPath);
                count++;
            }
            if (TaskSchedulerCk.IsChecked == true)
            {
                AutostartManager.AddToTaskScheduler(appPath);
                count++;
            }
            if (ServiceCk.IsChecked == true)
            {
                AutostartManager.AddToService(appPath);
                count++;
            }
            if (WinIniCk.IsChecked == true)
            {
                AutostartManager.AddToWinIni(appPath);
                count++;
            }
            Log($"✅ Applied {count} autorun methods", Colors.LightGreen);
            MessageBox.Show($"Applied {count} autorun methods.", "Success", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        private void RemoveAutostart_Click(object sender, RoutedEventArgs e)
        {
            var result = MessageBox.Show(
                "Remove all X6ProUnLocker autorun entries?",
                "Confirm",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;

            AutostartManager.RemoveAll();
            Log("✅ All autorun entries removed", Colors.LightGreen);
        }

        private void ScanForViruses_Click(object sender, RoutedEventArgs e)
        {
            Log("🦠 Starting full system scan... (simulated)", Colors.HotPink);
            // Здесь можно реализовать обход дисков и вызов VirusScanner для каждого файла
            Log("✅ Scan completed (simulated)", Colors.LightGreen);
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
            });
        }

        private void Log(string message) => Log(message, Colors.White);
    }
}