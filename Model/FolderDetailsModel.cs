using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCopyService.Model
{
    internal class FolderDetailsModel
    {
        public string FolderName { get; set; }
        public int FileCount { get; set; }  
        public int FolderCount { get; set; }
        public string ParentFolder {  get; set; } = null;
        public string OriginalLocation { get; set; }
        public int Watchstatus { get; set; } = 1;

    }
}
