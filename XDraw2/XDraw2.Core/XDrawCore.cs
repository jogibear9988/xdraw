using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.CompilerServices;
using System.ComponentModel;

[assembly: InternalsVisibleTo("XDraw2")]

namespace XDraw
{
   public class XDrawCore : INotifyPropertyChanged
   {
      static XDrawCore()
      {
         Instance = new XDrawCore();
      }

      public static XDrawCore Instance { get; private set; }

      public XDrawCore()
      {
         DockManager = null;
      }

      public IDockManager DockManager { get; internal set; }


      private object _PropertiesObject = null;

      public object PropertiesObject
      {
         get { return _PropertiesObject; }
         set
         {
            if (value != _PropertiesObject)
            {
               _PropertiesObject = value;
               NotifyPropertyChanged("PropertiesObject");
            }
         }
      }

      private IEnumerable<INewItemType> _ToolboxItems = null;

      public IEnumerable<INewItemType> ToolboxItems
      {
         get { return _ToolboxItems; }
         set
         {
            _ToolboxItems = value;
            NotifyPropertyChanged("ToolboxItems");
         }
      }

      public static readonly string DragDropItemType = "XDrawDragDropItem";

      private void NotifyPropertyChanged(string propertyName)
      {
         var temp = PropertyChanged;

         if (temp != null)
         {
            temp(this, new PropertyChangedEventArgs(propertyName));
         }
      }

      public event PropertyChangedEventHandler PropertyChanged;
   }
}
