using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Media;
using System.Windows.Controls;
using System.Windows;
using System.ComponentModel;

namespace XDraw.Xaml
{
   public class DrawingCompanion : Companion
   {
      public DrawingCompanion(object item) :
         base(item)
      {
         CreateFrameworkElement();
      }

      protected virtual void CreateFrameworkElement()
      {
         FrameworkElement = new Image()
         {
            Source = new DrawingImage(Drawing)
         };
      }

      /*[Category("Layout")]
      public Point Position
      {
         get { return ; }
         set
         {
            if (value != GeometryDrawing.Brush)
            {
               GeometryDrawing.Brush = value;
               NotifyPropertyChanged("Brush");
            }
         }
      }*/

      [Browsable(false)]
      public Drawing Drawing
      {
         get { return Item as Drawing; }
      }

      public override bool AutoApplyPosition
      {
         get { return true; }
      }

      protected override Rect CalcOuterBounds()
      {
         return Drawing != null ? Drawing.Bounds : Rect.Empty;
      }

      protected override Rect CalcVertexBounds()
      {
         return Drawing != null ? Drawing.Bounds : Rect.Empty;
      }

      public virtual void UpdateCustomGeometryProperty()
      { }
   }
}
