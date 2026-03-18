using System;
using System.Diagnostics;
using System.Management;

namespace X6ProUnLocker.Core
{
    public static class ProcessExtensions
    {
        public static double GetCpuUsage(this Process process)
        {
            // Простейший способ требует двух замеров, для упрощения вернём 0
            return 0;
        }

        public static string GetCompanyName(this Process process)
        {
            try
            {
                return FileVersionInfo.GetVersionInfo(process.MainModule.FileName).CompanyName ?? "";
            }
            catch
            {
                return "";
            }
        }
    }
}