using System;
using System.Collections.Generic;
using System.Linq;
using System.Media;
using System.Text;
using System.Threading.Tasks;
using Notifications.Wpf;
using Sharlayan.Core;

namespace ffxiv_chat_monitor.Helpers
{
    class NotificationHelper
    {
        private NotificationManager _notificationManager = new NotificationManager();

        public void ToastNotifyUser(ChatLogItem message)
        {
            // if our hooked copy of ffxiv isn't the foreground window, send a toast notification
            if (MainWindow.processHwid != MemoryHelper.GetForegroundWindow())
            {
                var notificationContent = new NotificationContent { Message = message.Line, Type = NotificationType.Information };
                _notificationManager.Show(notificationContent, onClick: () => MemoryHelper.SetForegroundWindow(MainWindow.processHwid));
            }
        }

        public void SoundNotifyUser()
        {
            SoundPlayer player = new SoundPlayer("notification.wav");
            player.LoadCompleted += delegate {
                player.Play();
            };
            player.LoadAsync();
        }
    }
}
