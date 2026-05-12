using System;
using System.Diagnostics;
using System.Management;

namespace X6ProUnLocker.Core
{
    public static class ProcessExtensions
    {
        public static double GetCpuUsage(this Process process)
        {
            return 0;
        }

        public static string GetCompanyName(this Process process)
        {
            try
            {
                return process.MainModule?.FileVersionInfo.CompanyName ?? "";
            }
            catch
            {
                return "";
            }
        }
    }
}