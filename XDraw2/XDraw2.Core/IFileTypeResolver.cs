using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XDraw
{
   public interface IFileTypeResolver
   {
      string FileTypeName { get; }

      IEnumerable<string> FileExtensions { get; }

      bool CanOpenFile(string path);

      IDockDocument OpenFile(string path);

      bool IsSameFile(IDockDocument document, string path);

      IDockDocument CreateNewFile();
   }
}
