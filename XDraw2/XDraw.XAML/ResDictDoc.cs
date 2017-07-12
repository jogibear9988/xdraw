using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using IOPath = System.IO.Path;
using System.Collections;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Xml.Linq;
using System.Xaml;
using System.Windows;
using System.Windows.Markup;
using System.Globalization;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace XDraw.Xaml
{
   public class ResDictDoc : INotifyPropertyChanged
   {
      internal static ResDictDoc Load(string path)
      {
         var doc = new ResDictDoc();
         doc.Load_(path);
         return doc;
      }

      public ResDictDoc()
      { }      

      private void Load_(string path)
      {
         _FileName = path;

         try
         {
            var xDoc = XDocument.Load(FileName);

            foreach (var xElement in xDoc.Root.Elements())
            {
               AddItem(ResDictElement.FromXElement(this, xElement));
            }
            SetModified(false);
         }
         catch (Exception ex)
         {

         }

         NotifyPropertyChanged("FileName");
         NotifyPropertyChanged("Name");
      }

      private string _FileName = String.Empty;

      public string FileName 
      { 
         get { return String.IsNullOrEmpty(_FileName) ? "New" : _FileName; }
      }

      public string Name
      {
         get
         {
            if (String.IsNullOrEmpty(FileName))
            {
               return "New";
            }
            else
            {
               return IOPath.GetFileName(FileName);
            }
         }
      }

      private bool _IsModified = false;

      internal void SetModified(bool isModified = true)
      {
         _IsModified = isModified;
         if (!_IsModified)
         {
            foreach (var item in Items)
            {
               item.SetModified(false);
            }
         }
         NotifyPropertyChanged("IsModified");
      }

      public bool IsModified
      {
         get { return _IsModified; }
      }

      private ObservableCollection<ResDictElement> _Items = new ObservableCollection<ResDictElement>();

      public IList<ResDictElement> Items
      {
         get { return _Items; }
      }

      public void AddItem(ResDictElement item)
      {
         _Items.Add(item);
         item.PropertyChanged += Item_PropertyChanged;
         SetModified();
      }

      public void RemoveItem(ResDictElement item)
      {
         if(_Items.Contains(item))
         {
            item.PropertyChanged -= Item_PropertyChanged;
            _Items.Remove(item);
            SetModified();
         }
      }

      private void Item_PropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         if (e.PropertyName == "IsModified" && (sender as ResDictElement).IsModified)
         {
            SetModified();
         }
      }

      internal ResDictElement AddFromClipboard()
      {
         if (Clipboard.ContainsText())
         {
            var text = Clipboard.GetText().Trim();
            int n = Math.Min(text.IndexOf('>'), text.IndexOf("/>"));
            if (text.StartsWith("<") && n > 0)
            {
               // add namespaces
               if (!text.Contains(" xmlns=\""))
               {
                  text = text.Insert(n, " xmlns=\"http://schemas.microsoft.com/winfx/2006/xaml/presentation\"");
               }
               if (!text.Contains(" xmlns:x=\""))
               {
                  text = text.Insert(n, " xmlns:x=\"http://schemas.microsoft.com/winfx/2006/xaml\"");
               }

               // add or modify key
               if (!text.Contains(" x:Key=\""))
               {
                  text = text.Insert(n, " x:Key=\"" + GenerateUniqueKey("Paste") + "\"");
               }
               else
               {
                  n = text.IndexOf(" x:Key=\"") + 8;
                  int n2 = text.IndexOf('\"', n);
                  if (n2 > n)
                  {
                     var key = text.Substring(n, n2 - n);
                     text = text.Remove(n, key.Length);
                     text = text.Insert(n, GenerateUniqueKey(key, "Copy"));
                  }
               }

               var xElement = XElement.Parse(text);
               var e = ResDictElement.FromXElement(this, xElement);
               e.SetModified(true, true);
               AddItem(e);

               return e;
            }
         }
         return null;
      }

      private string GenerateUniqueKey(string keyBase, string counterPrefix = "")
      {
         var key = keyBase;
         int n = 0;
         while (FindItemByKey(key) != null)
         {
            if(!String.IsNullOrEmpty(counterPrefix))
            {
               key = String.Format("{0}{1}{2}", keyBase, counterPrefix, ++n);
            }
            else
            {
               key = String.Format("{0}{1}", keyBase, ++n);
            }
         }
         return key;
      }

      public ResDictElement FindItemByKey(string key)
      {
         foreach (var item in Items)
         {
            if (String.CompareOrdinal(item.Key, key) == 0)
            {
               return item;
            }
         }
         return null;
      }

      #region INotifyPropertyChanged Members

      private void NotifyPropertyChanged(string propertyName)
      {
         var temp = PropertyChanged;
         if (temp != null)
         {
            temp(this, new PropertyChangedEventArgs(propertyName));
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;

      #endregion

      public ResDictElement AddNewDrawing()
      {
         var e = DrawingImageResDictElement.CreateNew(this, GenerateUniqueKey("New"));
         AddItem(e);
         return e;
      }
   }
}
