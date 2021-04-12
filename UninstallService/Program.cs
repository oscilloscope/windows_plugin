using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace UninstallService
{
    class Program
    {
        static void Main(string[] args)
        {
            var proc = new Process();
            proc.StartInfo.FileName = "pGina.InstallUtil.exe";
            proc.StartInfo.Arguments = "post-uninstall";

            proc.StartInfo.CreateNoWindow = false;
            proc.StartInfo.UseShellExecute = false;
            proc.Start();
            proc.WaitForExit();
            var exitCode = proc.ExitCode;
            proc.Close();
        }
    }
}
