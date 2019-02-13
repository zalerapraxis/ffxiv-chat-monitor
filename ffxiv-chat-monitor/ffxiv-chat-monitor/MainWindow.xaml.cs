using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Media;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using ffxiv_chat_monitor.Helpers;
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
        public static IntPtr processHwid;

        private CancellationTokenSource _cancelWatchTokenSource;

        MemoryHelper memoryHelper = new MemoryHelper();
        NotificationHelper notificationHelper = new NotificationHelper();
        SettingsHelper settingsHelper = new SettingsHelper();


        public MainWindow()
        {
            InitializeComponent();

            memoryHelper.GetMemoryProcessList();
            memoryHelper.HookMemoryProcess(0);
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

        private void BtnSettings_Click(object sender, RoutedEventArgs e)
        {
            SettingsWindow settingWindow = new SettingsWindow(settingsHelper, memoryHelper);
            settingWindow.Show();
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
                        // change this dictionary to whichever one we store our desired watch codes with - this one has all of them
                        if (settingsHelper._chatCodesWatchingDictionary.Contains(chatEntry.Code))
                        {
                            LogChatEntry(chatEntry);

                            // need to use this whenever we're referencing UI elements from other threads
                            Application.Current.Dispatcher.Invoke(() =>
                            {
                                if (chkbxToastNotification.IsChecked == true)
                                    notificationHelper.ToastNotifyUser(chatEntry);

                                if (chkbxSoundNotification.IsChecked == true)
                                    notificationHelper.SoundNotifyUser();
                            });
                        }
                    }
                }

                await Task.Delay(2000);
            }
        }


        private void LogChatEntry(ChatLogItem message)
        {
            // need to use this whenever we're referencing UI elements from other threads
            Application.Current.Dispatcher.Invoke(() =>
            {
                if (txtblkLog.Text.Length > 5000)
                    txtblkLog.Text = String.Empty;

                txtblkLog.Text = txtblkLog.Text + Environment.NewLine + $"{message.Code} - {message.Line}";
                txtblkScrollViewer.ScrollToEnd();
            });
        }       
    }
}
