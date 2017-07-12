using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows;
using System.ComponentModel;
using System.Windows.Data;
using XDraw.Controls;

namespace XDraw.Xaml
{
   public class GeometryDrawingCompanion : DrawingCompanion
   {
      public GeometryDrawingCompanion(object item) :
         base(item)
      {
         DependencyPropertyDescriptor.FromProperty(GeometryDrawing.BrushProperty, typeof(GeometryDrawing)).AddValueChanged(GeometryDrawing, OnBrushChanged);
         DependencyPropertyDescriptor.FromProperty(GeometryDrawing.PenProperty, typeof(GeometryDrawing)).AddValueChanged(GeometryDrawing, OnPenChanged);
      }

      public override object PropertiesObject
      {
         get { return this; }
      }

      private void OnBrushChanged(object sender, EventArgs e)
      {
         NotifyPropertyChanged("Brush");
      }

      private void OnPenChanged(object sender, EventArgs e)
      {
         // if the pen thickness has changed, then the OuterBounds has changed as well. So we need to call GeometryChanged
         GeometryChanged();
         NotifyPropertyChanged("Pen");
      }

      [Category("Appearance")]
      public Brush Brush
      {
         get { return GeometryDrawing.Brush; }
         set
         {
            if (value != GeometryDrawing.Brush)
            {
               GeometryDrawing.Brush = value;
               NotifyPropertyChanged("Brush");
            }
         }
      }

      [Category("Appearance")]
      public Pen Pen
      {
         get { return GeometryDrawing.Pen; }
         set
         {
            if (value != GeometryDrawing.Pen)
            {
               GeometryDrawing.Pen = value;
               NotifyPropertyChanged("Pen");
            }
         }
      }

      [Browsable(false)]
      public GeometryDrawing GeometryDrawing
      { get { return Drawing as GeometryDrawing; } }

      protected override void CreateFrameworkElement()
      {
         FrameworkElement = new GeometryShape(GeometryDrawing);
      }

      public override bool AutoApplyPosition
      {
         get { return false; }
      }

      protected override Rect CalcVertexBounds()
      {
         return GeometryDrawing != null ?  GeometryDrawing.Geometry.Bounds : Rect.Empty;
      }
   }
}
