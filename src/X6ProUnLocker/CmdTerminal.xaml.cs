using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;

namespace X6ProUnLocker
{
    public partial class CmdTerminal : UserControl, IDisposable
    {
        private Process cmdProcess;
        private List<string> commandHistory = new();
        private int historyIndex = -1;
        private StringBuilder currentOutput = new();

        public CmdTerminal()
        {
            InitializeComponent();
            StartCmd();
            commandHistory.AddRange(new[] { "dir", "cd", "cls", "ipconfig", "tasklist", "netstat", "systeminfo" });
        }

        private void StartCmd()
        {
            cmdProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "cmd.exe",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                    WorkingDirectory = @"C:\"
                }
            };
            cmdProcess.OutputDataReceived += (s, e) => AppendOutput(e.Data);
            cmdProcess.ErrorDataReceived += (s, e) => AppendOutput("❌ " + e.Data);
            cmdProcess.Start();
            cmdProcess.BeginOutputReadLine();
            cmdProcess.BeginErrorReadLine();
            AppendOutput("X6ProUnLocker CMD Terminal v1.0");
            AppendOutput("Copyright (c) 2026 POCO Systems");
            AppendOutput("");
        }

        private void AppendOutput(string text)
        {
            if (string.IsNullOrEmpty(text)) return;
            Dispatcher.Invoke(() =>
            {
                Paragraph para = new Paragraph();
                para.Inlines.Add(text);
                OutputBox.Document.Blocks.Add(para);
                OutputBox.ScrollToEnd();
            });
        }

        private void ExecuteCommand(string command)
        {
            if (string.IsNullOrWhiteSpace(command)) return;
            AppendOutput("C:\\> " + command);
            if (command.Equals("cls", StringComparison.OrdinalIgnoreCase))
            {
                OutputBox.Document.Blocks.Clear();
            }
            else if (command.StartsWith("cd ", StringComparison.OrdinalIgnoreCase))
            {
                // Смена директории обрабатывается отдельно
                string dir = command.Substring(3).Trim();
                try
                {
                    Directory.SetCurrentDirectory(Path.Combine(cmdProcess.StartInfo.WorkingDirectory, dir));
                    cmdProcess.StartInfo.WorkingDirectory = Directory.GetCurrentDirectory();
                    AppendOutput("Current directory: " + Directory.GetCurrentDirectory());
                }
                catch (Exception ex)
                {
                    AppendOutput("❌ " + ex.Message);
                }
            }
            else
            {
                cmdProcess.StandardInput.WriteLine(command);
            }

            if (!commandHistory.Contains(command))
                commandHistory.Insert(0, command);
            historyIndex = -1;
        }

        private void InputLine_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                ExecuteCommand(InputLine.Text);
                InputLine.Clear();
            }
            else if (e.Key == Key.Up)
            {
                if (historyIndex < commandHistory.Count - 1)
                {
                    historyIndex++;
                    InputLine.Text = commandHistory[historyIndex];
                    InputLine.CaretIndex = InputLine.Text.Length;
                }
            }
            else if (e.Key == Key.Down)
            {
                if (historyIndex > 0)
                {
                    historyIndex--;
                    InputLine.Text = commandHistory[historyIndex];
                    InputLine.CaretIndex = InputLine.Text.Length;
                }
                else if (historyIndex == 0)
                {
                    historyIndex = -1;
                    InputLine.Clear();
                }
            }
        }

        private void ExecuteButton_Click(object sender, RoutedEventArgs e) => ExecuteCommand(InputLine.Text);
        private void ClearButton_Click(object sender, RoutedEventArgs e) => OutputBox.Document.Blocks.Clear();

        public void Dispose()
        {
            if (cmdProcess != null && !cmdProcess.HasExited)
            {
                cmdProcess.Kill();
                cmdProcess.Dispose();
            }
        }
    }
}