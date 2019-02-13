using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Media;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Notifications.Wpf;
using Sharlayan;
using Sharlayan.Core;
using Sharlayan.Models;
using Sharlayan.Models.ReadResults;

namespace ffxiv_chat_monitor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        // globals used to track what parts of chat we've already read so we don't pull anything unnecessary
        int _previousArrayIndex = 0;
        int _previousOffset = 0;
        // store ffxiv process for later, when we want to focus window
        private IntPtr processHwid;

        private CancellationTokenSource _cancelWatchTokenSource;
        private readonly NotificationManager _notificationManager = new NotificationManager();

        public MainWindow()
        {
            InitializeComponent();

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
                Process process = processes[0];
                ProcessModel processModel = new ProcessModel
                {
                    Process = process,
                    IsWin64 = true
                };
                MemoryHandler.Instance.SetProcess(processModel, gameLanguage, patchVersion, useLocalCache);
                processHwid = process.MainWindowHandle;
            }
        }

        private void btnStartChatWatch_Click(object sender, RoutedEventArgs e)
        {
            // each time we stop, we dispose of the tokensource, so make a new one every time instead of making a new one at the global declaration
            _cancelWatchTokenSource = new CancellationTokenSource();
            CancellationToken cancellationToken = _cancelWatchTokenSource.Token;
            Task.Run(() => WatchChatLog(cancellationToken));
        }

        private void BtnStopChatWatch_Click(object sender, RoutedEventArgs e)
        {
            _cancelWatchTokenSource.Cancel();
            // free this token from memory, else we'll start leaking - we make a new one each time we press start anyway
            _cancelWatchTokenSource.Dispose();
        }

        private async void WatchChatLog(CancellationToken cancelToken)
        {   
            while (true)
            {
                if (cancelToken.IsCancellationRequested)
                    break;

                ChatLogResult readResult = Reader.GetChatLog(_previousArrayIndex, _previousOffset);

                var chatLogEntries = readResult.ChatLogItems;

                _previousArrayIndex = readResult.PreviousArrayIndex;
                _previousOffset = readResult.PreviousOffset;

                // only trigger the following code if the most recent chatlogs contain chat from certain linkshells, or private messages
                if (chatLogEntries.Count > 0)
                {
                    foreach (var chatEntry in chatLogEntries)
                    {
                        // 000C: PM's | 0016: LS7
                        if (chatEntry.Code == "000C" || chatEntry.Code == "0016" | chatEntry.Line.Contains("test"))
                        {
                            LogChatEntry(chatEntry);

                            // need to use this whenever we're referencing UI elements from other threads
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (chkbxToastNotification.IsChecked == true)
                                    ToastNotifyUser(chatEntry);

                                if (chkbxSoundNotification.IsChecked == true)
                                    SoundNotifyUser();
                            });
                        }
                    }
                }

                await Task.Delay(2000);
            }
        }

        private void ToastNotifyUser(ChatLogItem message)
        {
            // if our hooked copy of ffxiv isn't the foreground window, send a toast notification
            if (processHwid != GetForegroundWindow())
            {
                var notificationContent = new NotificationContent { Message = message.Line, Type = NotificationType.Information };
                _notificationManager.Show(notificationContent, onClick: () => SetForegroundWindow(processHwid));
            }
        }

        private void SoundNotifyUser()
        {
            SoundPlayer player = new SoundPlayer("notification.wav");
            player.LoadCompleted += delegate {
                player.Play();
            };
            player.LoadAsync();
        }

        private void LogChatEntry(ChatLogItem message)
        {
            // need to use this whenever we're referencing UI elements from other threads
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (txtblkLog.Text.Length > 5000)
                    txtblkLog.Text = String.Empty;

                txtblkLog.Text = txtblkLog.Text + Environment.NewLine + $"{message.TimeStamp} - {message.Line}";
                txtblkScrollViewer.ScrollToEnd();
            });
        }

        [DllImport("user32.dll")]
        static extern bool SetForegroundWindow(IntPtr hWnd);

        [DllImport("user32.dll")]
        private static extern IntPtr GetForegroundWindow();
    }
}
