using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.ComponentModel;

namespace XDraw
{
   /// <summary>
   /// Interaction logic for PropertiesToolPane.xaml
   /// </summary>
   public partial class PropertiesToolPane : UserControl, IDockPane, INotifyPropertyChanged
   {
      public PropertiesToolPane(object context, string id)
      {
         _Context = context;
         _PaneId = id;
         InitializeComponent();
      }

      #region IDockPane Members

      private object _Context;
      private string _PaneId;

      public object PaneContext
      {
         get { return _Context; }
      }

      public string PaneId
      {
         get { return _PaneId; }
      }

      public object PaneContent
      {
         get { return this; }
      }

      public string PaneTitle
      {
         get { return "Properties"; }
      }

      public string PaneToolTip
      {
         get { return null; }
      }

      public string PaneIconSource
      {
         get { return null; }
      }

      private bool _IsPaneVisible = true;

      public bool IsPaneVisible
      {
         get { return _IsPaneVisible; }
         set
         {
            if (value != _IsPaneVisible)
            {
               _IsPaneVisible = value;
               NotifyPropertyChanged("IsPaneVisible");
            }
         }
      }

      public PaneLocation DefaultPaneLocation
      {
         get { return PaneLocation.Right; }
      }

      public bool DefaultAutoHidePane
      {
         get { return false; }
      }

      public bool HidePaneOnClose
      {
         get { return true; }
      }

      public void PaneClosing(ref bool cancel)
      {
         
      }

      public void PaneClosed()
      {
         
      }

      #endregion

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
