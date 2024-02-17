using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCopyService
{
    internal class SharedCollectionClass
    {
        public static List<string> AllFolderList { get; set; } = new List<string>();

        public static List<string> AllFileList { get; set; } = new List<string>();

    }
}
