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
using System.Xml.Linq;
using Microsoft.Win32;
using System.IO;

namespace remes.XDraw
{
   /// <summary>
   /// Interaction logic for XDrawWindow.xaml
   /// </summary>
   public partial class XDrawWindow : Window
   {
      public static readonly DependencyProperty ToolbarButtonSizeProperty = DependencyProperty.Register(
         "ToolbarButtonSize", typeof(double), typeof(XDrawWindow), new PropertyMetadata(24.0));

      public XDrawWindow()
      {
         InitializeComponent();
      }

      private void Exit_Click(object sender, RoutedEventArgs e)
      {
         Close();
      }

      public XDrawing Drawing
      {
         get
         {
            if (tab.SelectedItem != null)
            {
               return ((tab.SelectedItem as TabItem).DataContext as XProjectDrawing).Drawing;
            }
            return null;
         }
      }

      public double ToolbarButtonSize
      {
         get { return (double)GetValue(ToolbarButtonSizeProperty); }
         set { SetValue(ToolbarButtonSizeProperty, value); }
      }

      private void Window_Loaded(object sender, RoutedEventArgs e)
      {
         // load window position and state
         var state = Properties.Settings.Default.XDrawWindow_State;
         if (!String.IsNullOrEmpty(state))
         {
            try
            {
               WindowState = (WindowState)Enum.Parse(typeof(WindowState), state);
            }
            catch { }
         }
         var location = new Rect(
            Properties.Settings.Default.XDrawWindow_Left,
            Properties.Settings.Default.XDrawWindow_Top,
            Properties.Settings.Default.XDrawWindow_Width,
            Properties.Settings.Default.XDrawWindow_Height);

         if (location.Width > 0.0 && location.Height > 0.0)
         {
            Left = location.Left;
            Top = location.Top;
            Width = location.Width;
            Height = location.Height;
         }

         NewProject(false);

         var startupProjectFile = (Application.Current as App).StartupProjectFile;
         if (!String.IsNullOrEmpty(startupProjectFile) && File.Exists(startupProjectFile))
         {
            m_Project.Load(startupProjectFile);
         }

         CheckModeButton();
      }
      
      private void Window_Closing(object sender, CancelEventArgs e)
      {
         // save Window position and state
         Properties.Settings.Default.XDrawWindow_State = WindowState.ToString();
         Properties.Settings.Default.XDrawWindow_Left = RestoreBounds.Left;
         Properties.Settings.Default.XDrawWindow_Top = RestoreBounds.Top;
         Properties.Settings.Default.XDrawWindow_Width = RestoreBounds.Width;
         Properties.Settings.Default.XDrawWindow_Height = RestoreBounds.Height;
      }

      private void DrawingModeChanged(object sender, EventArgs e)
      {
         CheckModeButton();
      }

      private void CheckModeButton()
      {
         if (Drawing != null)
         {
            //btnSelect.IsChecked = Drawing.Mode == XDrawingModes.Select;
            btnEdit.IsChecked = Drawing.Mode == XDrawingModes.Edit;
            btnLine.IsChecked = Drawing.Mode == XDrawingModes.NewLine;
            btnRect.IsChecked = Drawing.Mode == XDrawingModes.NewRect;
            btnEllipse.IsChecked = Drawing.Mode == XDrawingModes.NewEllipse;
            btnPath.IsChecked = Drawing.Mode == XDrawingModes.NewPath;
            btnText.IsChecked = Drawing.Mode == XDrawingModes.NewText;

            //miSelect.IsChecked = Drawing.Mode == XDrawingModes.Select;
            miEdit.IsChecked = Drawing.Mode == XDrawingModes.Edit;
            miNewLine.IsChecked = Drawing.Mode == XDrawingModes.NewLine;
            miNewRect.IsChecked = Drawing.Mode == XDrawingModes.NewRect;
            miNewEllipse.IsChecked = Drawing.Mode == XDrawingModes.NewEllipse;
            miNewPath.IsChecked = Drawing.Mode == XDrawingModes.NewPath;
            miNewText.IsChecked = Drawing.Mode == XDrawingModes.NewText;
         }
      }

      /*private void btnSelect_Click(object sender, RoutedEventArgs e)
      {
         if (Drawing != null)
         {
            Drawing.Mode = XDrawingModes.Select;
            CheckModeButton();
         }
      }*/

      private void btnEdit_Click(object sender, RoutedEventArgs e)
      {
         if (Drawing != null)
         {
            Drawing.Mode = XDrawingModes.Edit;
            CheckModeButton();
         }
      }

      private void btnNewLine_Click(object sender, RoutedEventArgs e)
      {
         if (Drawing != null)
         {
            Drawing.Mode = XDrawingModes.NewLine;
            CheckModeButton();
         }
      }

      private void btnNewRect_Click(object sender, RoutedEventArgs e)
      {
         if (Drawing != null)
         {
            Drawing.Mode = XDrawingModes.NewRect;
            CheckModeButton();
         }
      }

      private void btnNewEllipse_Click(object sender, RoutedEventArgs e)
      {
         if (Drawing != null)
         {
            Drawing.Mode = XDrawingModes.NewEllipse;
            CheckModeButton();
         }
      }

      private void btnNewPath_Click(object sender, RoutedEventArgs e)
      {
         if (Drawing != null)
         {
            Drawing.Mode = XDrawingModes.NewPath;
            CheckModeButton();
         }
      }

      private void btnNewText_Click(object sender, RoutedEventArgs e)
      {
         if (Drawing != null)
         {
            Drawing.Mode = XDrawingModes.NewText;
            CheckModeButton();
         }
      }

      private void btnLoad_Click(object sender, RoutedEventArgs e)
      {
         var ofd = new OpenFileDialog();
         ofd.Filter = "XDraw project files (*.xdprj)|*.xdprj|All files (*.*)|*.*";
         ofd.FilterIndex = 0;
         var result = ofd.ShowDialog(this);
         if (result.HasValue && result.Value)
         {
            NewProject(false);
            m_Project.Load(ofd.FileName);
         }
      }

      private void btnSave_Click(object sender, RoutedEventArgs e)
      {
         if (m_Project != null)
         {
            if (String.IsNullOrEmpty(m_Project.XAMLFilePath))
            {
               MessageBox.Show("XAML path must be set for project before saving!");
            }
            else
            {
               if (String.IsNullOrEmpty(m_Project.ProjectFilePath))
               {
                  var sfd = new SaveFileDialog();
                  sfd.DefaultExt = ".xdprj";
                  sfd.Filter = "XDraw project files (*.xdprj)|*.xdprj|All files (*.*)|*.*";
                  sfd.FilterIndex = 0;
                  var result = sfd.ShowDialog(this);
                  if (result.HasValue && result.Value)
                  {
                     m_Project.Save(sfd.FileName);
                  }
               }
               else
               {
                  m_Project.Save();
               }
            }
         }
      }

      private void btnNew_Click(object sender, RoutedEventArgs e)
      {
         NewProject(true);
      }

      private XDrawProject m_Project = null;

      private void NewProject(bool showDialog)
      {
         CloseAllTabs();
         projectTree.Items.Clear();

         var prj = new XDrawProject();
         prj.ProjectName = "New project";
         if (showDialog)
         {
            var dlg = new NewProjectDialog();
            dlg.Owner = this;
            dlg.Project = prj;
            var result = dlg.ShowDialog();
            if (!result.HasValue || !result.Value)
            {
               return;
            }
         }
         m_Project = prj;
         projectTree.Items.Add(m_Project);
      }

      private void AddDrawing_Click(object sender, RoutedEventArgs e)
      {
         if (m_Project != null)
         {
            var pDrawing = new XProjectDrawing();

            int n = 0;
            var name = "newDrawing";
            while (GetDrawing(name) != null)
            {
               name = String.Format("newDrawing_{0}", ++n);
            }
            pDrawing.DrawingName = name;
            pDrawing.ExportFormat = m_Project.DefaultExportFormat;
            m_Project.Drawings.Add(pDrawing);
            projectTree.ExpandObject(m_Project);
            projectTree.SelectObject(pDrawing);
            OpenDrawing(pDrawing);
         }
      }

      private XProjectDrawing GetDrawing(string name)
      {
         if (m_Project != null)
         {
            foreach (var drw in m_Project.Drawings)
            {
               if (String.Compare(name, drw.DrawingName) == 0)
               {
                  return drw;
               }
            }
         }
         return null;
      }


      private void PathSegmentTypeChange_Click(object sender, RoutedEventArgs e)
      {
         var shape = Drawing.SelectedShape as XDrawingPath;
         if (shape != null && Drawing.SelectedControlPoint != null && Drawing.SelectedControlPoint.Tag is PathSegment)
         {
            Drawing.SelectedControlPoint = shape.ChangeSegmentType(Drawing.SelectedControlPoint, (string)(e.OriginalSource as RadioButton).Tag);
         }
      }

      private void AddNewPathPtBefore_Click(object sender, RoutedEventArgs e)
      {
         var shape = Drawing.SelectedShape as XDrawingPath;
         if (shape != null && Drawing.SelectedControlPoint != null && Drawing.SelectedControlPoint.Tag != null)
         {
            Drawing.SelectedControlPoint = shape.AddSegment(Drawing.SelectedControlPoint.Tag, false);
         }
      }

      private void AddNewPathPtAfter_Click(object sender, RoutedEventArgs e)
      {
         var shape = Drawing.SelectedShape as XDrawingPath;
         if (shape != null && Drawing.SelectedControlPoint != null && Drawing.SelectedControlPoint.Tag != null)
         {
            Drawing.SelectedControlPoint = shape.AddSegment(Drawing.SelectedControlPoint.Tag, true);
         }
      }

      private void Button_Click(object sender, RoutedEventArgs e)
      {
         var shape = Drawing.SelectedShape as XDrawingPath;
         if (shape != null && Drawing.SelectedControlPoint != null && Drawing.SelectedControlPoint.Tag is PathSegment)
         {
            shape.DeleteSegment(Drawing.SelectedControlPoint);
         }
      }

      private void miSmallButtons_Click(object sender, RoutedEventArgs e)
      {
         ToolbarButtonSize = 24.0;
         miSmallButtons.IsChecked = true;
         miLargeButtons.IsChecked = false;
      }

      private void miLargeButtons_Click(object sender, RoutedEventArgs e)
      {
         ToolbarButtonSize = 48.0;
         miSmallButtons.IsChecked = false;
         miLargeButtons.IsChecked = true;
      }

      private void miAbout_Click(object sender, RoutedEventArgs e)
      {
         var aboutDlg = new AboutDialog();
         aboutDlg.Owner = this;
         aboutDlg.ShowDialog();
      }

      private void GridOptions_Click(object sender, RoutedEventArgs e)
      {
         if (Drawing != null)
         {
            var dlg = new GridOptionsDialog();
            dlg.Owner = this;
            dlg.Drawing = Drawing;
            dlg.ShowDialog();
         }
      }

      private void projectTree_MouseDoubleClick(object sender, MouseButtonEventArgs e)
      {
         try
         {
            var dObj = e.OriginalSource as DependencyObject;
            while (dObj != null && !(dObj is TreeViewItem))
            {
               dObj = VisualTreeHelper.GetParent(dObj);
            }
            if (dObj != null)
            {
               var pDrawing = (dObj as TreeViewItem).Header as XProjectDrawing;
               if (pDrawing != null)
               {
                  OpenDrawing(pDrawing);
               }
            }
         }
         catch (Exception ex)
         {
            MessageBox.Show(ex.Message);
         }
      }

      private void OpenDrawing(XProjectDrawing pDrawing)
      {
         TabItem ti = null;
         foreach (TabItem ti_ in tab.Items)
         {
            if (ti_.DataContext == pDrawing)
            {
               ti = ti_;
               break;
            }
         }

         if (ti == null)
         {
            ti = new TabItem();
            ti.DataContext = pDrawing;

            var sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            var tb = new TextBlock();
            tb.VerticalAlignment = VerticalAlignment.Center;
            BindingOperations.SetBinding(tb, TextBlock.TextProperty, new Binding("DrawingName"));
            sp.Children.Add(tb);
            var cb = new Button();
            var img = new Image();
            img.Source = FindResource("CloseImage") as ImageSource;
            cb.Content = img;
            cb.Click += CloseTab_Click;
            cb.Width = 16;
            cb.Height = 16;
            cb.Margin = new Thickness(5, 0, 0, 0);
            cb.Style = FindResource(ToolBar.ButtonStyleKey) as Style;
            sp.Children.Add(cb);
            ti.Header = sp;

            var scrollViewer = new ScrollViewer();
            scrollViewer.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;
            scrollViewer.Background = new SolidColorBrush(Colors.Gray);

            var canvas = new Canvas();

            scrollViewer.Content = canvas;
            ti.Content = scrollViewer;

            var drawing = pDrawing.GetDrawing(canvas);
            DependencyPropertyDescriptor.FromProperty(XDrawing.ModeProperty, typeof(XDrawing)).AddValueChanged(drawing, DrawingModeChanged);
            tab.Items.Add(ti);
         }
         tab.SelectedItem = ti;
         CheckModeButton();
      }

      void CloseTab_Click(object sender, RoutedEventArgs e)
      {
         var dObj = e.OriginalSource as DependencyObject;
         while (dObj != null && !(dObj is TabItem))
         {
            dObj = VisualTreeHelper.GetParent(dObj);
         }
         var ti = dObj as TabItem;
         if (ti != null)
         {
            CloseTab(ti);
         }
      }

      private void CloseTab(TabItem ti)
      {
         var pDrawing = ti.DataContext as XProjectDrawing;
         pDrawing.CloseDrawing();
         tab.Items.Remove(ti);
      }

      private void CloseAllTabs()
      {
         while (tab.Items.Count > 0)
         {
            CloseTab(tab.Items[tab.Items.Count - 1] as TabItem);
         }
      }

      private void ZoomIn_Click(object sender, RoutedEventArgs e)
      {
         if (Drawing != null)
         {
            Drawing.ZoomIn();
         }
      }

      private void ZoomOut_Click(object sender, RoutedEventArgs e)
      {
         if (Drawing != null)
         {
            Drawing.ZoomOut();
         }
      }

      private void Zoom1_Click(object sender, RoutedEventArgs e)
      {
         if (Drawing != null)
         {
            Drawing.Zoom1();
         }
      }

      private void TreeViewItem_MouseRightButtonDown(object sender, MouseEventArgs e)
      {
         TreeViewItem item = sender as TreeViewItem;
         if (item != null)
         {
            item.Focus();
            e.Handled = true;
         }
      }

      private void ExportDrawing_Click(object sender, RoutedEventArgs e)
      {
         var pDrawing = projectTree.SelectedItem as XProjectDrawing;
         if (pDrawing != null)
         {
            var dlg = new ExportDrawingDialog();
            dlg.Owner = this;
            bool closeDrawing = false;
            if (pDrawing.Drawing != null)
            {
               dlg.Drawing = pDrawing.Drawing;
            }
            else
            {
               var canvas = new Canvas();
               dlg.Drawing = pDrawing.GetDrawing(canvas);
               closeDrawing = true;
            }
            dlg.ShowDialog();
            if (closeDrawing)
            {
               pDrawing.CloseDrawing();
            }
         }
      }

      private void ImportDrawing_Click(object sender, RoutedEventArgs e)
      {
         var pDrawing = projectTree.SelectedItem as XProjectDrawing;
         if (pDrawing != null)
         {
            var dlg = new ImportDrawingDialog();
            dlg.Owner = this;
            bool closeDrawing = false;
            if (pDrawing.Drawing != null)
            {
               dlg.Drawing = pDrawing.Drawing;
            }
            else
            {
               var canvas = new Canvas();
               dlg.Drawing = pDrawing.GetDrawing(canvas);
               closeDrawing = true;
            }
            dlg.ShowDialog();
            if (closeDrawing)
            {
               pDrawing.CloseDrawing();
            }
         }
      }

      private void RemoveDrawing_Click(object sender, RoutedEventArgs e)
      {
         var pDrawing = projectTree.SelectedItem as XProjectDrawing;
         if (pDrawing != null)
         {
            var result = MessageBox.Show(
               this,
               "Do you really want to delete the selected drawing?",
               "Delete drawing",
               MessageBoxButton.YesNo,
               MessageBoxImage.Question);
            if (result == MessageBoxResult.Yes)
            {
               foreach (TabItem ti_ in tab.Items)
               {
                  if (ti_.DataContext == pDrawing)
                  {
                     CloseTab(ti_);
                     break;
                  }
               }
               m_Project.RemoveDrawing(pDrawing);
            }
         }
      }

      private void CopyDrawing_Click(object sender, RoutedEventArgs e)
      {
         if (m_Project != null)
         {
            var pDrawing = projectTree.SelectedItem as XProjectDrawing;
            if (pDrawing != null)
            {
               var newDrawing = new XProjectDrawing();
               newDrawing.DrawingName = String.Format("copy of {0}", pDrawing.DrawingName);
               newDrawing.ExportFormat = pDrawing.ExportFormat;
               newDrawing.DrawingSize = pDrawing.DrawingSize;
               pDrawing.StoreXmlCode();
               newDrawing.XmlCode = new XElement(pDrawing.XmlCode);
               m_Project.Drawings.Add(newDrawing);
               projectTree.SelectObject(newDrawing);
               OpenDrawing(newDrawing);
            }
         }
      }

      private void tab_SelectionChanged(object sender, SelectionChangedEventArgs e)
      {
         CheckModeButton();
      }

      private void BoldText_Initialized(object sender, EventArgs e)
      {
         if (Drawing != null)
         {
            var txt = Drawing.SelectedShape as XDrawingText;
            if (txt != null)
            {
               (sender as CheckBox).IsChecked = (txt.FontWeight == FontWeights.Bold);
            }
         }
      }

      private void BoldText_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
      {
         if (Drawing != null)
         {
            var txt = Drawing.SelectedShape as XDrawingText;
            if (txt != null)
            {
               (sender as CheckBox).IsChecked = (txt.FontWeight == FontWeights.Bold);
            }
         }
      }

      private void BoldText_Click(object sender, RoutedEventArgs e)
      {
         if (Drawing != null)
         {
            var txt = Drawing.SelectedShape as XDrawingText;
            if(txt != null)
            {
               if ((e.OriginalSource as CheckBox).IsChecked == true)
               {
                  txt.FontWeight = FontWeights.Bold;
               }
               else
               {
                  txt.FontWeight = FontWeights.Normal;
               }
            }
         }
      }

      private void ItalicText_Initialized(object sender, EventArgs e)
      {
         if (Drawing != null)
         {
            var txt = Drawing.SelectedShape as XDrawingText;
            if (txt != null)
            {
               (sender as CheckBox).IsChecked = (txt.FontStyle == FontStyles.Italic);
            }
         }
      }

      private void ItalicText_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
      {
         if (Drawing != null)
         {
            var txt = Drawing.SelectedShape as XDrawingText;
            if (txt != null)
            {
               (sender as CheckBox).IsChecked = (txt.FontStyle == FontStyles.Italic);
            }
         }
      }

      private void ItalicText_Click(object sender, RoutedEventArgs e)
      {
         if (Drawing != null)
         {
            var txt = Drawing.SelectedShape as XDrawingText;
            if (txt != null)
            {
               if ((e.OriginalSource as CheckBox).IsChecked == true)
               {
                  txt.FontStyle = FontStyles.Italic;
               }
               else
               {
                  txt.FontStyle = FontStyles.Normal;
               }
            }
         }
      }
   }
}
