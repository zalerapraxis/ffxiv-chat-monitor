using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ffxiv_chat_monitor.Helpers
{
    public class SettingsHelper
    {
        // contains all relevant chat code IDs, for use in settings to select what chats to watch
        internal OrderedDictionary _chatCodeReferenceDictionary = new OrderedDictionary
        {
            { "0010", "Linkshell 1" },
            { "0011", "Linkshell 2" },
            { "0012", "Linkshell 3" },
            { "0013", "Linkshell 4" },
            { "0014", "Linkshell 5" },
            { "0015", "Linkshell 6" },
            { "0016", "Linkshell 7" },
            { "0017", "Linkshell 8" },
            { "000A", "Say chat" },
            { "000C", "Private Messages" }
            // get fc chat code eventually
        };

        // contains only the codes IDs we want to program to watch for
        internal OrderedDictionary _chatCodesWatchingDictionary = new OrderedDictionary
        {
            { "0016", "Linkshell 7" },
            { "000C", "Private Messages" }
        };
    }
}
