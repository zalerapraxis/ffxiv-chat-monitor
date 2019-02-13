using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using ffxiv_chat_monitor.Helpers;

namespace ffxiv_chat_monitor
{
    /// <summary>
    /// Interaction logic for SettingsWindow.xaml
    /// </summary>
    public partial class SettingsWindow : Window
    {
        public SettingsWindow(SettingsHelper settingsHelper)
        {
            InitializeComponent();

            // add code reference dictionary entries to reference listbox
            foreach (DictionaryEntry entry in settingsHelper._chatCodeReferenceDictionary)
            {
                lstbxChatCodeReference.Items.Add(entry.Value);
            }

            foreach (DictionaryEntry entry in settingsHelper._chatCodesWatchingDictionary)
            {
                lstbxChatCodeWatchlist.Items.Add(entry.Value);
            }
        }

        private void lstbxChatCodeReference_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int chatCodeReferenceIndex = lstbxChatCodeReference.SelectedIndex;
            //lstbxChatCodeReference.Items.RemoveAt(chatCodeReferenceIndex);


        }
    }
}
