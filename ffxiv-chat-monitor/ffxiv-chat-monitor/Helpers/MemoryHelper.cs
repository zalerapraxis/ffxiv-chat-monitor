using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Sharlayan;
using Sharlayan.Models;

namespace ffxiv_chat_monitor.Helpers
{
    public class MemoryHelper
    {
        internal OrderedDictionary processList = new OrderedDictionary();

        public void GetMemoryProcessList()
        {
            // clear any existing items in processList, get list of processes, add each process's pid and start time to a dict
            processList.Clear();
            Process[] processes = Process.GetProcessesByName("ffxiv_dx11");
            foreach (var process in processes)
            {
                processList.Add(process.StartTime, process.MainWindowHandle);
            }
        }

        public void HookMemoryProcess(int index)
        {
            // hook sharlayan into ffxiv process memory
            // eventually we'll need to make a way to select a specific process since we often run multiple clients
            Process[] processes = Process.GetProcessesByName("ffxiv_dx11");
            if (processes.Length > 0)
            {
                // supported: English, Chinese, Japanese, French, German, Korean
                string gameLanguage = "English";
                // whether to always hit API on start to get the latest sigs based on patchVersion, or use the local json cache (if the file doesn't exist, API will be hit)
                bool useLocalCache = true;
                // patchVersion of game, or latest
                string patchVersion = "latest";
                Process process = processes[index];
                ProcessModel processModel = new ProcessModel
                {
                    Process = process,
                    IsWin64 = true
                };
                MemoryHandler.Instance.SetProcess(processModel, gameLanguage, patchVersion, useLocalCache);
                MainWindow.processHwid = process.MainWindowHandle;
            }
        }

        [DllImport("user32.dll")]
        internal static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        internal static extern IntPtr GetForegroundWindow();
    }
}
