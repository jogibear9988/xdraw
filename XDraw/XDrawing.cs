using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Shapes;
using System.Windows.Media;
using System.Windows;
using System.Windows.Input;
using System.ComponentModel;
using System.Reflection;
using System.Xml.Linq;
using System.Globalization;

namespace remes.XDraw
{
   public enum XDrawingModes
   {
      //Select,
      Edit,
      NewLine,
      NewRect,
      NewEllipse,
      NewPath,
      NewText
   }

   public enum XDrawingExportFormat
   {
      Canvas,
      DrawingImage
   }

   public class XDrawing : DependencyObject, IDisposable
   {
      public static readonly DependencyProperty SelectedShapeProperty = DependencyProperty.Register(
         "SelectedShape", typeof(XDrawingShape), typeof(XDrawing), new PropertyMetadata(null));
      
      public static readonly DependencyProperty SelectedControlPointProperty = DependencyProperty.Register(
         "SelectedControlPoint", typeof(ControlPoint), typeof(XDrawing), new PropertyMetadata(null));
      
      public static readonly DependencyProperty ModeProperty = DependencyProperty.Register(
         "Mode", typeof(XDrawingModes), typeof(XDrawing), new PropertyMetadata(XDrawingModes.Edit/*Select*/, ModeChanged));

      public static readonly DependencyProperty IsGridVisibleProperty = DependencyProperty.Register(
         "IsGridVisible", typeof(bool), typeof(XDrawing), new PropertyMetadata(true, IsGridVisibleChanged));

      public static readonly DependencyProperty IsSnapToGridEnabledProperty = DependencyProperty.Register(
         "IsSnapToGridEnabled", typeof(bool), typeof(XDrawing), new PropertyMetadata(true, IsSnapToGridEnabledChanged));

      public static readonly DependencyProperty PointGridModeProperty = DependencyProperty.Register(
         "PointGridMode", typeof(bool), typeof(XDrawing), new PropertyMetadata(false, PointGridModeChanged));

      public static readonly DependencyProperty SnapGridWidthProperty = DependencyProperty.Register(
         "SnapGridWidth", typeof(double), typeof(XDrawing), new PropertyMetadata(1.0, SnapGridWidthChanged));

      public static readonly DependencyProperty DisplayGridWidthProperty = DependencyProperty.Register(
         "DisplayGridWidth", typeof(double), typeof(XDrawing), new PropertyMetadata(10.0, DisplayGridWidthChanged));


      public double ZoomFactor
      {
         get { return (double)GetValue(ZoomFactorProperty); }
         set { SetValue(ZoomFactorProperty, value); }
      }

      public static readonly DependencyProperty ZoomFactorProperty =
          DependencyProperty.Register("ZoomFactor", typeof(double), typeof(XDrawing), new UIPropertyMetadata(1.0, ZoomFactorChanged));

      private static void ZoomFactorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         var drawing = d as XDrawing;

         drawing.OnZoomFactorChanged(e);
      }

      private void OnZoomFactorChanged(DependencyPropertyChangedEventArgs e)
      {
         Canvas.LayoutTransform = new ScaleTransform((double)e.NewValue, (double)e.NewValue);
         (m_GridBrush.Drawing as GeometryDrawing).Pen.Thickness = 1.0 / (double)e.NewValue;
         (((m_PointGridBrush.Drawing as DrawingGroup).Children[1] as GeometryDrawing).Geometry as EllipseGeometry).RadiusX = 0.6 / (double)e.NewValue;
         (((m_PointGridBrush.Drawing as DrawingGroup).Children[1] as GeometryDrawing).Geometry as EllipseGeometry).RadiusY = 0.6 / (double)e.NewValue;

         m_ControlPointPath.StrokeThickness = 1.0 / (double)e.NewValue;
         m_SelectedControlPointPath.StrokeThickness = 2.0 / (double)e.NewValue;
         m_ControlLinePath.StrokeThickness = 1.0 / (double)e.NewValue;

         foreach (var cp in m_ControlPoints)
         {
            cp.ZoomChanged((double)e.NewValue);
         }
      }

      private DrawingBrush m_GridBrush = null;
      private DrawingBrush m_PointGridBrush = null;

      private ContextMenu m_ContextMenu = null;

      public XDrawing(Canvas canvas)
      {
         Canvas = canvas;

         Canvas.LayoutTransform = new ScaleTransform(ZoomFactor, ZoomFactor);

         Canvas.PreviewMouseDown += Canvas_PreviewMouseDown;
         Canvas.PreviewMouseUp += Canvas_PreviewMouseUp;
         Canvas.PreviewMouseMove += Canvas_PreviewMouseMove;

         m_GridBrush = new DrawingBrush(
            new GeometryDrawing(
               new SolidColorBrush(Colors.White),
               new Pen(new SolidColorBrush(Colors.LightGray), 1.0),
               new RectangleGeometry(new Rect(0, 0, DisplayGridWidth, DisplayGridWidth))));
         m_GridBrush.Stretch = Stretch.None;
         m_GridBrush.TileMode = TileMode.Tile;
         m_GridBrush.Viewport = new Rect(0.0, 0.0, DisplayGridWidth, DisplayGridWidth);
         m_GridBrush.ViewportUnits = BrushMappingMode.Absolute;

         var dg = new DrawingGroup();
         dg.Children.Add(
            new GeometryDrawing(
               new SolidColorBrush(Colors.White),
               new Pen(new SolidColorBrush(Colors.White), 1),
               new RectangleGeometry(new Rect(0, 0, DisplayGridWidth, DisplayGridWidth))));
         dg.Children.Add(
            new GeometryDrawing(
               new SolidColorBrush(Colors.Black),
               null,
               new EllipseGeometry(new Point(0.0, 0.0), 0.6, 0.6)));
         
         m_PointGridBrush = new DrawingBrush(dg);

         m_PointGridBrush.Stretch = Stretch.None;
         m_PointGridBrush.TileMode = TileMode.Tile;
         m_PointGridBrush.Viewport = new Rect(0.0, 0.0, DisplayGridWidth, DisplayGridWidth);
         m_PointGridBrush.ViewportUnits = BrushMappingMode.Absolute;

         SetDisplayGrid();

         Canvas.Children.Clear();

         m_ControlPointPath = new Path();
         m_ControlPointPath.Stroke = new SolidColorBrush(Colors.Blue);
         m_ControlPointPath.StrokeThickness = 1.0;
         m_ControlPointPath.Fill = new SolidColorBrush(Colors.White);
         m_ControlPointPath.Opacity = 0.5;
         m_ControlPointGroup = new GeometryGroup();
         m_ControlPointPath.Data = m_ControlPointGroup;
         Canvas.Children.Add(m_ControlPointPath);

         m_SelectedControlPointPath = new Path();
         m_SelectedControlPointPath.Stroke = new SolidColorBrush(Colors.Blue);
         m_SelectedControlPointPath.StrokeThickness = 2.0;
         m_SelectedControlPointPath.Fill = new SolidColorBrush(Colors.White);
         m_SelectedControlPointPath.Opacity = 0.5;
         m_SelectedControlPointGroup = new GeometryGroup();
         m_SelectedControlPointPath.Data = m_SelectedControlPointGroup;
         Canvas.Children.Add(m_SelectedControlPointPath);

         m_ControlLinePath = new Path();
         m_ControlLinePath.Stroke = new SolidColorBrush(Colors.Blue);
         m_ControlLinePath.StrokeThickness = 1.0;
         m_ControlLinePath.StrokeDashArray.Add(5.0);
         m_ControlLinePath.StrokeDashArray.Add(5.0);
         m_ControlLinePath.Opacity = 0.5;
         m_ControlLineGroup = new GeometryGroup();
         m_ControlLinePath.Data = m_ControlLineGroup;
         Canvas.Children.Add(m_ControlLinePath);

         Canvas.Focusable = true;

         m_ContextMenu = new ContextMenu();

         var mi = new MenuItem();
         mi.Header = "Cut";
         mi.Command = ApplicationCommands.Cut;
         mi.CommandTarget = Canvas;
         var img = new Image();
         img.Width = 16;
         img.Height = 16;
         img.Source = Canvas.FindResource("CutImage") as DrawingImage;
         mi.Icon = img;
         m_ContextMenu.Items.Add(mi);

         mi = new MenuItem();
         mi.Header = "Copy";
         mi.Command = ApplicationCommands.Copy;
         mi.CommandTarget = Canvas;
         img = new Image();
         img.Width = 16;
         img.Height = 16;
         img.Source = Canvas.FindResource("CopyImage") as DrawingImage;
         mi.Icon = img;
         m_ContextMenu.Items.Add(mi);

         mi = new MenuItem();
         mi.Header = "Paste";
         mi.Command = ApplicationCommands.Paste;
         mi.CommandTarget = Canvas;
         img = new Image();
         img.Width = 16;
         img.Height = 16;
         img.Source = Canvas.FindResource("PasteImage") as DrawingImage;
         mi.Icon = img;
         m_ContextMenu.Items.Add(mi);

         m_ContextMenu.Items.Add(new Separator());

         mi = new MenuItem();
         mi.Header = "Remove shape";
         mi.Command = ApplicationCommands.Delete;
         mi.CommandTarget = Canvas;
         img = new Image();
         img.Width = 16;
         img.Height = 16;
         img.Source = Canvas.FindResource("DeleteShapeImage") as DrawingImage;
         mi.Icon = img;
         m_ContextMenu.Items.Add(mi);

         m_ContextMenu.Items.Add(new Separator());

         mi = new MenuItem();
         mi.Header = "Send to back";
         mi.Command = XDrawingCommands.SendToBack;
         mi.CommandTarget = Canvas;
         img = new Image();
         img.Width = 16;
         img.Height = 16;
         img.Source = Canvas.FindResource("SendToBackImage") as DrawingImage;
         mi.Icon = img;
         m_ContextMenu.Items.Add(mi);

         mi = new MenuItem();
         mi.Header = "One to back";
         mi.Command = XDrawingCommands.OneToBack;
         mi.CommandTarget = Canvas;
         img = new Image();
         img.Width = 16;
         img.Height = 16;
         img.Source = Canvas.FindResource("OneToBackImage") as DrawingImage;
         mi.Icon = img;
         m_ContextMenu.Items.Add(mi);

         mi = new MenuItem();
         mi.Header = "One to front";
         mi.Command = XDrawingCommands.OneToFront;
         mi.CommandTarget = Canvas;
         img = new Image();
         img.Width = 16;
         img.Height = 16;
         img.Source = Canvas.FindResource("OneToFrontImage") as DrawingImage;
         mi.Icon = img;
         m_ContextMenu.Items.Add(mi);

         mi = new MenuItem();
         mi.Header = "Send to front";
         mi.Command = XDrawingCommands.SendToFront;
         mi.CommandTarget = Canvas;
         img = new Image();
         img.Width = 16;
         img.Height = 16;
         img.Source = Canvas.FindResource("SendToFrontImage") as DrawingImage;
         mi.Icon = img;
         m_ContextMenu.Items.Add(mi);

         Canvas.CommandBindings.Add(new CommandBinding(ApplicationCommands.Cut, Cut_Executed, Cut_CanExecute));
         Canvas.CommandBindings.Add(new CommandBinding(ApplicationCommands.Copy, Copy_Executed, Copy_CanExecute));
         Canvas.CommandBindings.Add(new CommandBinding(ApplicationCommands.Paste, Paste_Executed, Paste_CanExecute));
         Canvas.CommandBindings.Add(new CommandBinding(ApplicationCommands.Delete, Delete_Executed, Delete_CanExecute));

         Canvas.CommandBindings.Add(new CommandBinding(XDrawingCommands.SendToBack, SendToBack_Executed, SendToBack_CanExecute));
         Canvas.CommandBindings.Add(new CommandBinding(XDrawingCommands.OneToBack, OneToBack_Executed, OneToBack_CanExecute));
         Canvas.CommandBindings.Add(new CommandBinding(XDrawingCommands.OneToFront, OneToFront_Executed, OneToFront_CanExecute));
         Canvas.CommandBindings.Add(new CommandBinding(XDrawingCommands.SendToFront, SendToFront_Executed, SendToFront_CanExecute));
      }

      private void Cut_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         if (SelectedShape != null)
         {
            var shape = SelectedShape;
            SelectedShape = null;
            var xDoc = new XDocument(new XElement("x"));
            shape.Export(xDoc.Root, XDrawingExportFormat.DrawingImage);
            Clipboard.SetText(xDoc.Root.FirstNode.ToString());
            RemoveShape(shape);
         }
      }

      private void Cut_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = SelectedShape != null;
         e.Handled = true;
      }

      private void Copy_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         if (SelectedShape != null)
         {
            var xDoc = new XDocument(new XElement("x"));
            SelectedShape.Export(xDoc.Root, XDrawingExportFormat.DrawingImage);
            Clipboard.SetText(xDoc.Root.FirstNode.ToString());
         }
      }

      private void Copy_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = SelectedShape != null;
         e.Handled = true;
      }

      private void Paste_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         if (Clipboard.ContainsText() && Clipboard.GetText().StartsWith("<GeometryDrawing"))
         {
            try
            {
               var xaml = Clipboard.GetText();
               var xDoc = XDocument.Parse(xaml);

               XDrawingShape shape = XDrawingShape.LoadFromGeometryDrawing(this, xDoc.Root);
               if (shape != null)
               {
                  m_Shapes.Add(shape);
                  Canvas.Children.Insert(Canvas.Children.IndexOf(m_ControlPointPath), shape.Path);
                  SelectedShape = shape;
               }
            }
            catch (Exception ex)
            {
               MessageBox.Show(ex.Message, "Exception while pasting");
            }
         }
      }

      private void Paste_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         var fmt = DataFormats.GetDataFormat("XDrawShape");
         e.CanExecute = Clipboard.ContainsText() && Clipboard.GetText().StartsWith("<GeometryDrawing");
         e.Handled = true;
      }

      private void Delete_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         if (SelectedShape != null)
         {
            RemoveShape(SelectedShape);
         }
      }

      private void Delete_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = SelectedShape != null;
         e.Handled = true;
      }

      private void SendToBack_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         if (SelectedShape != null)
         {
            var n = m_Shapes.IndexOf(SelectedShape);
            if (n > 0)
            {
               m_Shapes.RemoveAt(n);
               m_Shapes.Insert(0, SelectedShape);

               n = Canvas.Children.IndexOf(SelectedShape.Path);
               Canvas.Children.RemoveAt(n);
               Canvas.Children.Insert(0, SelectedShape.Path);
            }
         }
      }

      private void SendToBack_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = SelectedShape != null;
         e.Handled = true;
      }

      private void OneToBack_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         if (SelectedShape != null)
         {
            var n = m_Shapes.IndexOf(SelectedShape);
            if (n > 0)
            {
               m_Shapes.RemoveAt(n);
               m_Shapes.Insert(n - 1, SelectedShape);

               n = Canvas.Children.IndexOf(SelectedShape.Path);
               Canvas.Children.RemoveAt(n);
               Canvas.Children.Insert(n - 1, SelectedShape.Path);
            }
         }
      }

      private void OneToBack_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = SelectedShape != null;
         e.Handled = true;
      }

      private void OneToFront_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         if (SelectedShape != null)
         {
            var n = m_Shapes.IndexOf(SelectedShape);
            if (n < m_Shapes.Count - 1)
            {
               m_Shapes.RemoveAt(n);
               m_Shapes.Insert(n + 1, SelectedShape);

               n = Canvas.Children.IndexOf(SelectedShape.Path);
               Canvas.Children.RemoveAt(n);
               Canvas.Children.Insert(n + 1, SelectedShape.Path);
            }
         }
      }

      private void OneToFront_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = SelectedShape != null;
         e.Handled = true;
      }

      private void SendToFront_Executed(object sender, ExecutedRoutedEventArgs e)
      {
         if (SelectedShape != null)
         {
            var n = m_Shapes.IndexOf(SelectedShape);
            if (n < m_Shapes.Count - 1)
            {
               m_Shapes.RemoveAt(n);
               m_Shapes.Add(SelectedShape);

               n = Canvas.Children.IndexOf(SelectedShape.Path);
               Canvas.Children.RemoveAt(n);
               Canvas.Children.Insert(Canvas.Children.IndexOf(m_ControlPointPath), SelectedShape.Path);
            }
         }
      }

      private void SendToFront_CanExecute(object sender, CanExecuteRoutedEventArgs e)
      {
         e.CanExecute = SelectedShape != null;
         e.Handled = true;
      }

      public XElement Export(XDrawingExportFormat format)
      {
         XElement xDrawing;
         XElement xParent;
         switch (format)
         {
            case XDrawingExportFormat.Canvas:
               xDrawing = new XElement("Canvas");
               xParent = xDrawing;
               break;

            case XDrawingExportFormat.DrawingImage:
               xParent = new XElement("DrawingGroup");

               xDrawing = new XElement("DrawingImage",
                  new XElement("DrawingImage.Drawing",
                     xParent));
               break;

            default:
               throw new ArgumentException("format not supported");
         }
         foreach (var shape in m_Shapes)
         {
            shape.Export(xParent, format);
         }
         return xDrawing;
      }

      /// <summary>
      /// Imports a drawing from xml.
      /// </summary>
      /// <param name="xElement">Xml element too start searching</param>
      /// <returns>true if all xml element could be loaded, false if unknown (skiped) elements have been found</returns>
      /// <remarks>
      /// Searches for the 1st DrawingImage, if none is found look for Canvas
      /// </remarks>
      public bool Import(XElement xRoot)
      {
         return Import(xRoot, true);
      }

      /// <summary>
      /// Imports a drawing from xml.
      /// </summary>
      /// <param name="xElement">Xml element too start searching</param>
      /// <param name="clearDrawing">remove all shapes before import</param>
      /// <returns>true if all xml element could be loaded, false if unknown (skiped) elements have been found</returns>
      /// <remarks>
      /// Searches for the 1st DrawingImage, if none is found look for Canvas
      /// </remarks>
      public bool Import(XElement xElement, bool clearDrawing)
      {
         if (clearDrawing)
         {
            ClearDrawing();
         }

         // look for DrawingImage
         var xRoot = (from e in xElement.DescendantsAndSelf("DrawingImage")
                      select e).FirstOrDefault();
         if (xRoot != null)
         {
            // we need the 1st DrawingGroup
            xRoot = (from e in xElement.Descendants("DrawingGroup")
                     select e).FirstOrDefault();

            if (xRoot != null)
            {
               foreach (var xGD in xRoot.Elements("GeometryDrawing"))
               {
                  XDrawingShape shape = XDrawingShape.LoadFromGeometryDrawing(this, xGD);
                  if (shape != null)
                  {
                     m_Shapes.Add(shape);
                     Canvas.Children.Insert(Canvas.Children.IndexOf(m_ControlPointPath), shape.Path);
                  }
               }
            }
         }
         else
         {
            // DrawingImage not found, look for Canvas
            xRoot = (from e in xElement.DescendantsAndSelf("Canvas")
                     select e).FirstOrDefault();

            if (xRoot != null)
            {
               foreach (var xPath in xRoot.Elements("Path"))
               {
                  XDrawingShape shape = XDrawingShape.LoadFromPath(this, xPath);
                  if(shape != null)
                  {
                     m_Shapes.Add(shape);
                     Canvas.Children.Insert(Canvas.Children.IndexOf(m_ControlPointPath), shape.Path);
                  }
               }
            }
         }

         return true;
      }

      public void RemoveShape(XDrawingShape shape)
      {
         if (SelectedShape == shape)
         {
            SelectedControlPoint = null;
            SelectedShape = null;
         }
         shape.Dispose();
         m_Shapes.Remove(shape);
      }

      public void ClearDrawing()
      {
         SelectedControlPoint = null;
         SelectedShape = null;
         foreach (var shape in Shapes)
         {
            shape.Dispose();
         }
         m_Shapes.Clear();
      }

      public XDrawingModes Mode
      {
         get { return (XDrawingModes)GetValue(ModeProperty); }
         set { SetValue(ModeProperty, value); }
      }

      private static void ModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         var drawing = d as XDrawing;
         if (drawing != null && drawing.SelectedShape != null && (XDrawingModes)e.NewValue != (XDrawingModes)e.OldValue)
         {
            drawing.ClearControlPoints();
            drawing.SelectedShape.CreateControlPoints(((XDrawingModes)e.NewValue) == XDrawingModes.Edit);
         }
      }

      private void SetDisplayGrid()
      {
         if (IsGridVisible)
         {
            if (PointGridMode)
            {
               Canvas.Background = m_PointGridBrush;
            }
            else
            {
               Canvas.Background = m_GridBrush;
            }
         }
         else
         {
            Canvas.Background = new SolidColorBrush(Colors.White);
         }
      }

      public bool IsGridVisible
      {
         get { return (bool)GetValue(IsGridVisibleProperty); }
         set { SetValue(IsGridVisibleProperty, value); }
      }

      private static void IsGridVisibleChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         var drawing = d as XDrawing;

         drawing.SetDisplayGrid();
      }

      public bool IsSnapToGridEnabled
      {
         get { return (bool)GetValue(IsSnapToGridEnabledProperty); }
         set { SetValue(IsSnapToGridEnabledProperty, value); }
      }

      private static void IsSnapToGridEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         var drawing = d as XDrawing;

      }

      public bool PointGridMode
      {
         get { return (bool)GetValue(PointGridModeProperty); }
         set { SetValue(PointGridModeProperty, value); }
      }

      private static void PointGridModeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         var drawing = d as XDrawing;

         drawing.SetDisplayGrid();
      }

      public double SnapGridWidth
      {
         get { return (double)GetValue(SnapGridWidthProperty); }
         set { SetValue(SnapGridWidthProperty, value); }
      }

      private static void SnapGridWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         var drawing = d as XDrawing;

      }

      public double DisplayGridWidth
      {
         get { return (double)GetValue(DisplayGridWidthProperty); }
         set { SetValue(DisplayGridWidthProperty, value); }
      }

      private static void DisplayGridWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         var drawing = d as XDrawing;

         drawing.OnDisplayGridWidthChanged(d, e);
      }

      protected virtual void OnDisplayGridWidthChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         var width = (double)e.NewValue;
         ((m_GridBrush.Drawing as GeometryDrawing).Geometry as RectangleGeometry).Rect = new Rect(0, 0, width, width);
         m_GridBrush.Viewport = new Rect(0.0, 0.0, width, width);

         (((m_PointGridBrush.Drawing as DrawingGroup).Children[0] as GeometryDrawing).Geometry as RectangleGeometry).Rect = new Rect(0.0, 0.0, width, width);
         m_PointGridBrush.Viewport = new Rect(0.0, 0.0, width, width);
      }

      private HitTestInfo? m_DragInfo = null;

      void Canvas_PreviewMouseDown(object sender, MouseButtonEventArgs e)
      {
         var pt = e.GetPosition(Canvas);

         Canvas.Focus();
         Keyboard.Focus(Canvas);


         if (e.ChangedButton == MouseButton.Left)
         {
            switch (Mode)
            {
               //case XDrawingModes.Select:
               case XDrawingModes.Edit:
                  m_DragInfo = HitTest(pt);
                  if (!m_DragInfo.HasValue)
                  {
                     SelectedShape = null;
                  }
                  else
                  {
                     if (m_DragInfo.Value.Shape != SelectedShape)
                     {
                        SelectedShape = m_DragInfo.Value.Shape;
                     }
                     if (m_DragInfo.Value.ControlPoint != SelectedControlPoint)
                     {
                        if (m_DragInfo.Value.ControlPoint == null || m_DragInfo.Value.ControlPoint.IsSelectable)
                        {
                           SelectedControlPoint = m_DragInfo.Value.ControlPoint;
                        }
                     }
                     m_DragInfo.Value.DragObject.StartDrag();
                  }
                  break;

               case XDrawingModes.NewLine:
               case XDrawingModes.NewRect:
               case XDrawingModes.NewEllipse:
               case XDrawingModes.NewPath:
               case XDrawingModes.NewText:
                  SelectedShape = null;
                  XDrawingModes newMode = Mode;
                  switch (Mode)
                  {
                     case XDrawingModes.NewLine:
                        m_DragInfo = XDrawingLine.CreateNewByDrag(this, SnapToGrid(pt), out newMode);
                        break;

                     case XDrawingModes.NewRect:
                        m_DragInfo = XDrawingRectangle.CreateNewByDrag(this, SnapToGrid(pt), out newMode);
                        break;

                     case XDrawingModes.NewEllipse:
                        m_DragInfo = XDrawingEllipse.CreateNewByDrag(this, SnapToGrid(pt), out newMode);
                        break;

                     case XDrawingModes.NewPath:
                        m_DragInfo = XDrawingPath.CreateNewByDrag(this, SnapToGrid(pt), out newMode);
                        break;

                     case XDrawingModes.NewText:
                        m_DragInfo = XDrawingText.CreateNewByDrag(this, SnapToGrid(pt), out newMode);
                        break;
                  }
                  if (m_DragInfo.HasValue)
                  {
                     Mode = newMode;
                     SetValue(SelectedShapeProperty, m_DragInfo.Value.Shape);
                     m_Shapes.Add(m_DragInfo.Value.Shape);
                     Canvas.Children.Insert(Canvas.Children.IndexOf(m_ControlPointPath), m_DragInfo.Value.Shape.Path);
                     m_DragInfo.Value.DragObject.StartDrag();
                  }
                  break;
            }
            Canvas.CaptureMouse();
         }
         else if (e.ChangedButton == MouseButton.Right)
         {
            var hti = HitTest(pt);
            if (!hti.HasValue)
            {
               SelectedShape = null;
            }
            else
            {
               if (hti.Value.Shape != SelectedShape)
               {
                  SelectedShape = hti.Value.Shape;
               }
               if (hti.Value.ControlPoint != SelectedControlPoint)
               {
                  if (hti.Value.ControlPoint == null || hti.Value.ControlPoint.IsSelectable)
                  {
                     SelectedControlPoint = hti.Value.ControlPoint;
                  }
               }
            }
         }
         e.Handled = true;
      }

      void Canvas_PreviewMouseUp(object sender, MouseButtonEventArgs e)
      {
         if (e.ChangedButton == MouseButton.Left)
         {
            if (m_DragInfo.HasValue)
            {
               m_DragInfo.Value.DragObject.EndDrag();
               m_DragInfo = null;
            }
            Canvas.ReleaseMouseCapture();
         }
         else if (e.ChangedButton == MouseButton.Right)
         {
            m_ContextMenu.IsOpen = true;
         }
         e.Handled = true;
      }

      void Canvas_PreviewMouseMove(object sender, MouseEventArgs e)
      {
         var pt = e.GetPosition(Canvas);

         if (m_DragInfo.HasValue)
         {
            m_DragInfo.Value.DragObject.DragObject(SnapToGrid(pt - m_DragInfo.Value.Offset));
         }
         e.Handled = true;
      }

      private Point SnapToGrid(Point pt)
      {
         if (IsSnapToGridEnabled && (Keyboard.Modifiers & ModifierKeys.Control) == 0)
         {
            pt.X = Math.Round(pt.X / SnapGridWidth) * SnapGridWidth;
            pt.Y = Math.Round(pt.Y / SnapGridWidth) * SnapGridWidth;
         }
         return pt;
      }

      public Canvas Canvas { get; private set; }

      private Path m_ControlPointPath;
      private GeometryGroup m_ControlPointGroup;
      private Path m_SelectedControlPointPath;
      private GeometryGroup m_SelectedControlPointGroup;
      private Path m_ControlLinePath;
      private GeometryGroup m_ControlLineGroup;

      internal GeometryGroup ControlPointGroup
      {
         get { return m_ControlPointGroup; }
      }

      internal GeometryGroup SelectedControlPointGroup
      {
         get { return m_SelectedControlPointGroup; }
      }

      internal GeometryGroup ControlLineGroup
      {
         get { return m_ControlLineGroup; }
      }

      public XDrawingShape SelectedShape
      {
         get { return (XDrawingShape)GetValue(SelectedShapeProperty); }
         set
         {
            if (SelectedShape != value)
            {
               ClearControlPoints();
               SetValue(SelectedShapeProperty, value);
               if (value != null)
               {
                  value.CreateControlPoints(Mode == XDrawingModes.Edit);
               }
            }
         }
      }

      public ControlPoint SelectedControlPoint
      {
         get { return (ControlPoint)GetValue(SelectedControlPointProperty); }
         set
         {
            if (SelectedControlPoint != value)
            {
               if (SelectedControlPoint != null)
               {
                  SelectedControlPoint.IsSelected = false;
               }
               if (value != null)
               {
                  value.IsSelected = true;
               }
               SetValue(SelectedControlPointProperty, value);
            }
         }
      }

      private List<ControlPoint> m_ControlPoints = new List<ControlPoint>();
      private List<XDrawingShape> m_Shapes = new List<XDrawingShape>();

      public IEnumerable<XDrawingShape> Shapes
      {
         get { return m_Shapes; }
      }

      public IEnumerable<XDrawingShape> InverseShapes
      {
         get
         {
            for (int n = m_Shapes.Count - 1; n >= 0; --n)
            {
               yield return m_Shapes[n];
            }
         }
      }

      internal void ClearControlPoints()
      {
         for (int n = m_ControlPoints.Count - 1; n >= 0; --n)
         {
            m_ControlPoints[n].Dispose();
         }
         m_ControlPoints.Clear();
      }

      internal void AddControlPoint(ControlPoint cp)
      {
         m_ControlPoints.Add(cp);
      }

      private bool IsControlPath(Path path)
      {
         return path != m_ControlPointPath && path != m_SelectedControlPointPath && path != m_ControlLinePath;
      }

      public IEnumerable<Path> GetPathObjects(bool inverse)
      {
         if (inverse)
         {
            for (int n = Canvas.Children.Count - 1; n >= 0; --n)
            {
               if (Canvas.Children[n] is Path && !IsControlPath(Canvas.Children[n] as Path))
               {
                  yield return Canvas.Children[n] as Path;
               }
            }
         }
         else
         {
            foreach (var c in Canvas.Children)
            {
               if (c is Path && c != m_ControlPoints)
               {
                  yield return c as Path;
               }
            }
         }
      }

      private HitTestInfo? HitTest(Point pt)
      {
         HitTestInfo? hti = null;
         foreach (var cp in m_ControlPoints)
         {
            hti = cp.HitTest(pt);
            if (hti.HasValue)
            {
               return hti;
            }
         }

         if (SelectedShape != null)
         {
            hti = SelectedShape.HitTest(pt);
            if (hti.HasValue)
            {
               return hti;
            }
         }

         foreach (var p in InverseShapes)
         {
            hti = p.HitTest(pt);
            if (hti.HasValue)
            {
               return hti;
            }
         }
         return null;
      }

      public void ZoomIn()
      {
         ZoomFactor *= 1.25;
      }

      public void ZoomOut()
      {
         ZoomFactor /= 1.25;
      }

      public void Zoom1()
      {
         ZoomFactor = 1.0;
      }

      #region IDisposable Members

      ~XDrawing()
      {
         Dispose(false);
      }

      protected virtual void Dispose(bool disposing)
      {
         if (disposing)
         {
            ClearDrawing();
         }
      }

      public void Dispose()
      {
         Dispose(true);
      }

      #endregion
   }

   public interface IDragableObject
   {
      HitTestInfo? HitTest(Point pt);
      void StartDrag();
      void DragObject(Point pt);
      void EndDrag();
      bool IsDraged { get; }
   }

   public struct HitTestInfo
   {
      public XDrawingShape Shape;
      public ControlPoint ControlPoint;
      public Vector Offset;

      public IDragableObject DragObject
      {
         get { return (ControlPoint as IDragableObject) ?? (Shape as IDragableObject); }
      }
   }
}
