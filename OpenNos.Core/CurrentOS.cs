// CurrentOS Class by blez Detects the current OS (Windows, Linux, MacOS)

using System;
using System.Diagnostics;
using System.IO;
// ReSharper disable UnusedMember.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable MemberCanBePrivate.Global

namespace OpenNos.Core
{
    public static class CurrentOS
    {
        #region Instantiation

        static CurrentOS()
        {
            IsWindows = Path.DirectorySeparatorChar == '\\';
            if (IsWindows)
            {
                const string subKey = @"SOFTWARE\Wow6432Node\Microsoft\Windows NT\CurrentVersion";
                Microsoft.Win32.RegistryKey key = Microsoft.Win32.Registry.LocalMachine;
                Microsoft.Win32.RegistryKey skey = key.OpenSubKey(subKey);

                if (skey != null)
                {
                    string name = skey.GetValue("ProductName").ToString();
                    string mjVersion = skey.GetValue("CurrentMajorVersionNumber").ToString();
                    string mnVersion = skey.GetValue("CurrentMinorVersionNumber").ToString();
                    string cBuild = skey.GetValue("CurrentBuild").ToString();
                    string ubr = skey.GetValue("UBR").ToString();
                    Name = $"{name} [Version {mjVersion}.{mnVersion}.{cBuild}.{ubr} %bit]";
                }

                Name = Name.Replace("%bit", Is64BitWindows ? "64bit" : "32bit");

                if (Is64BitWindows)
                {
                    Is64Bit = true;
                }
                else
                {
                    Is32Bit = true;
                }
            }
            else
            {
                string unixName = ReadProcessOutput("uname");
                if (unixName.Contains("Darwin"))
                {
                    IsUnix = true;
                    IsMac = true;

                    Name = "macOS " + ReadProcessOutput("sw_vers", "-productVersion");
                    Name = Name.Trim();

                    string machine = ReadProcessOutput("uname", "-m");
                    if (machine.Contains("x86_64"))
                    {
                        Is64Bit = true;
                    }
                    else
                    {
                        Is32Bit = true;
                    }

                    Name += " " + (Is32Bit ? "32bit" : "64bit");
                }
                else if (unixName.Contains("Linux"))
                {
                    IsUnix = true;
                    IsLinux = true;

                    Name = ReadProcessOutput("lsb_release", "-d");
                    Name = Name.Substring(Name.IndexOf(":", StringComparison.Ordinal) + 1);
                    Name = Name.Trim();

                    string machine = ReadProcessOutput("uname", "-m");
                    if (machine.Contains("x86_64"))
                    {
                        Is64Bit = true;
                    }
                    else
                    {
                        Is32Bit = true;
                    }

                    Name += " " + (Is32Bit ? "32bit" : "64bit");
                }
                else if (unixName != "")
                {
                    IsUnix = true;
                }
                else
                {
                    IsUnknown = true;
                }
            }
        }

        #endregion

        #region Properties

        public static bool Is32Bit { get; }

        public static bool Is64Bit { get; }

        public static bool IsLinux { get; }

        public static bool IsMac { get; }

        public static bool IsUnix { get; }

        public static bool IsUnknown { get; }

        public static bool IsWindows { get; }

        public static string Name { get; }

        private static bool Is64BitWindows => Environment.Is64BitOperatingSystem;

        #endregion

        #region Methods

        private static string ReadProcessOutput(string name) => ReadProcessOutput(name, null);

        private static string ReadProcessOutput(string name, string args)
        {
            try
            {
                Process p = new Process
                {
                    StartInfo =
                    {
                        UseShellExecute = false,
                        RedirectStandardOutput = true
                    }
                };
                if (!string.IsNullOrEmpty(args))
                {
                    p.StartInfo.Arguments = " " + args;
                }

                p.StartInfo.FileName = name;
                p.Start();

                // Do not wait for the child process to exit before reading to the end of its
                // redirected stream. p.WaitForExit(); Read the output stream first and then wait.
                string output = p.StandardOutput.ReadToEnd();
                p.WaitForExit();

                return output.Trim();
            }
            catch
            {
                return "";
            }
        }

        #endregion
    }
}