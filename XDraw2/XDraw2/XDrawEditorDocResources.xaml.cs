using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace XDraw
{
   partial class XDrawEditorDocResources : INotifyPropertyChanged
   {
      public static XDrawEditorDocResources Instance { get; private set; }

      static XDrawEditorDocResources()
      {
         new XDrawEditorDocResources();
      }

      public XDrawEditorDocResources()
      {
         Instance = this;
         InitializeComponent();

         var npc = XDrawCore.Instance.DockManager as INotifyPropertyChanged;
         if (npc != null)
         {
            npc.PropertyChanged += DockManager_PropertyChanged;
         }

         // instanciate MergeMenu elemnts
         var dummy = this["XDrawEditorDocToolBar"];
         var d = dummy;
      }

      private void DockManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         if (e.PropertyName == "ActiveDocument")
         {
            NotifyPropertyChanged("ActiveEditorDoc");
         }
      }

      public XDrawEditorDoc ActiveEditorDoc
      {
         get { return XDrawCore.Instance.DockManager.ActiveDocument as XDrawEditorDoc; }
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
   }
}
