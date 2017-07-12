using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Xml.Linq;

namespace XDraw.Xaml
{
   public class ResDictFileTypeResolver : IFileTypeResolver
   {
      public string FileTypeName
      {
         get { return "Xaml Resource Dictionary"; }
      }

      public IEnumerable<string> FileExtensions
      {
         get { yield return ".xaml"; }
      }

      public bool CanOpenFile(string path)
      {
         if (String.Compare(Path.GetExtension(path), ".xaml", StringComparison.OrdinalIgnoreCase) == 0)
         {
            try
            {
               var xDoc = XDocument.Load(path);
               return (String.Compare(xDoc.Root.Name.NamespaceName, "http://schemas.microsoft.com/winfx/2006/xaml/presentation", StringComparison.OrdinalIgnoreCase) == 0 ||
                  String.Compare(xDoc.Root.Name.NamespaceName, "http://schemas.microsoft.com/netfx/2007/xaml/presentation", StringComparison.OrdinalIgnoreCase) == 0)
                  && String.Compare(xDoc.Root.Name.LocalName, "ResourceDictionary", StringComparison.OrdinalIgnoreCase) == 0;
            }
            catch { }
         }
         return false;
      }

      private int _NextDocId = 0;

      public IDockDocument OpenFile(string path)
      {
         var doc = ResDictDoc.Load(path);
         return XDrawCore.Instance.DockManager.ShowDocument(String.Format("ResDictDoc{0}", _NextDocId++), 
            (context, id) =>
            {
               return new ResDictDocControl(context, id)
                  {
                     Document = doc
                  };
            });
      }

      public bool IsSameFile(IDockDocument document, string path)
      {
         var docCtrl = document as ResDictDocControl;
         if(docCtrl != null)
         {
            return String.Compare(docCtrl.Document.FileName, path, StringComparison.OrdinalIgnoreCase) == 0;
         }
         return false;
      }

      public IDockDocument CreateNewFile()
      {
         return XDrawCore.Instance.DockManager.ShowDocument(String.Format("ResDictDoc{0}", _NextDocId++),
            (context, id) =>
            {
               return new ResDictDocControl(context, id)
               {
                  Document = new ResDictDoc()
               };
            });
      }
   }
}
