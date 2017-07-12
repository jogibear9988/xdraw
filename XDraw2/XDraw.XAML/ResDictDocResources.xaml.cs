using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

namespace XDraw.Xaml
{
   partial class ResDictDocResources : INotifyPropertyChanged
   {
      public static ResDictDocResources Instance { get; private set; }

      static ResDictDocResources()
      {
         new ResDictDocResources();
      }

      public ResDictDocResources()
      {
         Instance = this;
         InitializeComponent();

         var npc = XDrawCore.Instance.DockManager as INotifyPropertyChanged;
         if (npc != null)
         {
            npc.PropertyChanged += DockManager_PropertyChanged;
         }

         // instanciate MergeMenu elemnts
         var dummy = this["ResDictDocToolBar"];
         var d = dummy;
      }

      private void DockManager_PropertyChanged(object sender, PropertyChangedEventArgs e)
      {
         if (e.PropertyName == "ActiveDocument")
         {
            NotifyPropertyChanged("ActiveResDictDoc");
         }
      }

      public ResDictDocControl ActiveResDictDoc
      {
         get { return XDrawCore.Instance.DockManager.ActiveDocument as ResDictDocControl; }
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
