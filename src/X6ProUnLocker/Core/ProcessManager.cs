using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;

namespace X6ProUnLocker.Core
{
    public class ProcessInfo
    {
        public int Pid { get; set; }
        public string Name { get; set; } = "";
        public string? Path { get; set; }
        public long Memory { get; set; }
        public double CpuPercent { get; set; }
    }

    public static class ProcessManager
    {
        public static List<ProcessInfo> GetProcessList()
        {
            var list = new List<ProcessInfo>();
            IntPtr snapshot = WinApiNative.CreateToolhelp32Snapshot(WinApiNative.TH32CS_SNAPPROCESS, 0);
            if (snapshot == IntPtr.Zero) return list;

            var entry = new WinApiNative.PROCESSENTRY32 { dwSize = (uint)Marshal.SizeOf<WinApiNative.PROCESSENTRY32>() };
            if (!WinApiNative.Process32First(snapshot, ref entry))
            {
                WinApiNative.CloseHandle(snapshot);
                return list;
            }

            do
            {
                var info = new ProcessInfo { Pid = (int)entry.th32ProcessID, Name = entry.szExeFile };

                // Получаем путь с fallback'ом для совместимости
                IntPtr hProc = WinApiNative.OpenProcess(WinApiNative.PROCESS_QUERY_LIMITED_INFORMATION, false, entry.th32ProcessID);
                if (hProc != IntPtr.Zero)
                {
                    var sb = new StringBuilder(260);
                    int size = 260;
                    if (WinApiNative.QueryFullProcessImageName(hProc, 0, sb, ref size))
                        info.Path = sb.ToString();
                    else
                    {
                        var sb2 = new StringBuilder(260);
                        if (WinApiNative.GetModuleFileNameEx(hProc, IntPtr.Zero, sb2, (uint)sb2.Capacity) > 0)
                            info.Path = sb2.ToString();
                    }
                    WinApiNative.CloseHandle(hProc);
                }

                try
                {
                    var p = Process.GetProcessById(info.Pid);
                    info.Memory = p.WorkingSet64;
                    info.CpuPercent = 0; // Заглушка, реальное измерение требует двух выборок
                }
                catch { }

                list.Add(info);
            } while (WinApiNative.Process32Next(snapshot, ref entry));

            WinApiNative.CloseHandle(snapshot);
            return list;
        }
    }
}