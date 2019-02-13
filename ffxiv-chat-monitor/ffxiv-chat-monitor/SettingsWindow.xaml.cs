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
        private SettingsHelper _settingsHelper;
        private MemoryHelper _memoryHelper;
        public SettingsWindow(SettingsHelper settingsHelper, MemoryHelper memoryHelper)
        {
            InitializeComponent();

            // we set settingsHelper as a global var so we could access it throughout the class, but we need to
            // assign it with the instance we first built in the mainwindow so both windows use the same instnace
            _settingsHelper = settingsHelper;
            // same
            _memoryHelper = memoryHelper;

            // call this again when we open settings to make sure that the process list is up to date
            _memoryHelper.GetMemoryProcessList();

            // add code reference dictionary entries to reference listbox
            foreach (DictionaryEntry entry in settingsHelper._chatCodeReferenceDictionary)
            {
                lstbxChatCodeReference.Items.Add(entry.Value);
            }

            // add watched codes to the watchlist listbox
            foreach (DictionaryEntry entry in settingsHelper._chatCodesWatchingDictionary)
            {
                lstbxChatCodeWatchlist.Items.Add(entry.Value);
            }

            // 
            foreach (DictionaryEntry entry in _memoryHelper.processList)
            {
                cmbxProcessList.Items.Add($"{entry.Key} - {entry.Value}");
            }
        }

        private void lstbxChatCodeReference_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int chatCodeReferenceIndex = lstbxChatCodeReference.SelectedIndex;

            MoveEntryFromReferenceToWatchlist(chatCodeReferenceIndex);

        }

        private void lstbxChatCodeWatchlist_OnMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            int watchListIndex = lstbxChatCodeWatchlist.SelectedIndex;

            lstbxChatCodeWatchlist.Items.RemoveAt(watchListIndex);
            _settingsHelper._chatCodesWatchingDictionary.RemoveAt(watchListIndex);
        }

        private void BtnSelectProcess_Click(object sender, RoutedEventArgs e)
        {
            int processListindex = cmbxProcessList.SelectedIndex;

            _memoryHelper.HookMemoryProcess(processListindex);
        }

        private void MoveEntryFromReferenceToWatchlist(int index)
        {
            // get the dictionaryentry in our search results dictionary that matches that list
            var chatCodeReferenceDictEntry = _settingsHelper._chatCodeReferenceDictionary.Cast<DictionaryEntry>().ElementAt(index);

            // check for a duplicate in the watch dict, otherwise add selected dictionaryentry to the watchlist and add its key to the watcheditems listbox
            if (_settingsHelper._chatCodesWatchingDictionary.Contains(chatCodeReferenceDictEntry.Key))
            {
                MessageBox.Show("You've already got that item in your watch list.");
            }
            else
            {
                _settingsHelper._chatCodesWatchingDictionary.Add(chatCodeReferenceDictEntry.Key, chatCodeReferenceDictEntry.Value);
                lstbxChatCodeWatchlist.Items.Add((string) chatCodeReferenceDictEntry.Value);
            }
        }
    }
}
