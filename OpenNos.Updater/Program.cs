using System.Diagnostics;
using System.IO;
using System.Threading;

namespace OpenNos.Updater
{
    public static class Program
    {
        public static void Main()
        {
            string binary = string.Empty;
            foreach (string s in Directory.GetFiles("."))
            {
                string f = s.Replace("./", string.Empty).Replace(".\\", string.Empty);
                switch (f.ToLower())
                {
                    case "opennos.world.exe":
                    case "opennos.login.exe":
                    case "opennos.master.server.exe":
                    case "opennos.chatlog.server.exe":
                        binary = f;
                        break;
                }
            }

            Thread.Sleep(5000);

            if (Directory.Exists("updates"))
            {
                foreach (string s in Directory.GetFiles("updates/", "*" , SearchOption.AllDirectories))
                {
                    switch (s.ToLower().Replace("\\", "/").Replace("updates/", string.Empty))
                    {
                        case "opennos.updater.pdb":
                        case "opennos.updater.exe":
                        case "opennos.world.exe.config":
                        case "opennos.login.exe.config":
                        case "opennos.master.server.exe.config":
                        case "opennos.chatlog.server.exe.config":
                        case "log.xml":
                        case "missinglanguagekeys.txt":
                            break;
                        default:
                            File.Move(s, s.Replace("updates", "."));
                            break;
                    }
                }
            }

            if (!string.IsNullOrWhiteSpace(binary))
            {
                Process.Start(binary);
            }
        }
    }
}