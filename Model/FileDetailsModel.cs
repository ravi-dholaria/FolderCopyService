using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FolderCopyService.Model
{
    internal class FileDetailsModel
    {
        public int FolderId { get; set; }
        public string FileName { get; set; }
        public string FileExtension { get; set; }
        public int FileStatus { get; set; } = 1;
        public string FilePath { get; set; }
        public DateTime ModificationDate { get; set; }
    }
}
