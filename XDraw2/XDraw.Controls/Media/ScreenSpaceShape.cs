using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;
using System.ComponentModel;

namespace XDraw.Controls.Media
{
   [Localizability(LocalizationCategory.None, Readability = Readability.Unreadable)]
   public abstract class ScreenSpaceShape : FrameworkElement
   {
      // Fields
      private Pen _pen;
      private Geometry _renderedGeometry = Geometry.Empty;
      //[CommonDependencyProperty]
      public static readonly DependencyProperty FillProperty = DependencyProperty.Register(
         "Fill", typeof(Brush), typeof(ScreenSpaceShape), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender | FrameworkPropertyMetadataOptions.AffectsRender));
      //private static UncommonField<BoxedMatrix> StretchMatrixField = new UncommonField<BoxedMatrix>(null);
      private BoxedMatrix StretchMatrix = null;
      public static readonly DependencyProperty StretchProperty = DependencyProperty.Register(
         "Stretch", typeof(Stretch), typeof(ScreenSpaceShape), new FrameworkPropertyMetadata(Stretch.None, FrameworkPropertyMetadataOptions.AffectsArrange));
      public static readonly DependencyProperty StrokeDashArrayProperty = DependencyProperty.Register(
         "StrokeDashArray", typeof(DoubleCollection), typeof(ScreenSpaceShape), new FrameworkPropertyMetadata(new DoubleCollection(), FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(ScreenSpaceShape.OnPenChanged)));
      public static readonly DependencyProperty StrokeDashCapProperty = DependencyProperty.Register(
         "StrokeDashCap", typeof(PenLineCap), typeof(ScreenSpaceShape), new FrameworkPropertyMetadata(PenLineCap.Flat, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(ScreenSpaceShape.OnPenChanged)));//, new ValidateValueCallback(ValidateEnums.IsPenLineCapValid));
      public static readonly DependencyProperty StrokeDashOffsetProperty = DependencyProperty.Register(
         "StrokeDashOffset", typeof(double), typeof(ScreenSpaceShape), new FrameworkPropertyMetadata(0.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(ScreenSpaceShape.OnPenChanged)));
      public static readonly DependencyProperty StrokeEndLineCapProperty = DependencyProperty.Register(
         "StrokeEndLineCap", typeof(PenLineCap), typeof(ScreenSpaceShape), new FrameworkPropertyMetadata(PenLineCap.Flat, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(ScreenSpaceShape.OnPenChanged)));//, new ValidateValueCallback(ValidateEnums.IsPenLineCapValid));
      public static readonly DependencyProperty StrokeLineJoinProperty = DependencyProperty.Register(
         "StrokeLineJoin", typeof(PenLineJoin), typeof(ScreenSpaceShape), new FrameworkPropertyMetadata(PenLineJoin.Miter, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(ScreenSpaceShape.OnPenChanged)));//, new ValidateValueCallback(ValidateEnums.IsPenLineJoinValid));
      public static readonly DependencyProperty StrokeMiterLimitProperty = DependencyProperty.Register(
         "StrokeMiterLimit", typeof(double), typeof(ScreenSpaceShape), new FrameworkPropertyMetadata(10.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(ScreenSpaceShape.OnPenChanged)));
      //[CommonDependencyProperty]
      public static readonly DependencyProperty StrokeProperty = DependencyProperty.Register(
         "Stroke", typeof(Brush), typeof(ScreenSpaceShape), new FrameworkPropertyMetadata(null, FrameworkPropertyMetadataOptions.SubPropertiesDoNotAffectRender | FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(ScreenSpaceShape.OnPenChanged)));
      public static readonly DependencyProperty StrokeStartLineCapProperty = DependencyProperty.Register(
         "StrokeStartLineCap", typeof(PenLineCap), typeof(ScreenSpaceShape), new FrameworkPropertyMetadata(PenLineCap.Flat, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(ScreenSpaceShape.OnPenChanged)));//, new ValidateValueCallback(ValidateEnums.IsPenLineCapValid));
      //[CommonDependencyProperty]
      public static readonly DependencyProperty StrokeThicknessProperty = DependencyProperty.Register(
         "StrokeThickness", typeof(double), typeof(ScreenSpaceShape), new FrameworkPropertyMetadata(1.0, FrameworkPropertyMetadataOptions.AffectsRender | FrameworkPropertyMetadataOptions.AffectsMeasure, new PropertyChangedCallback(ScreenSpaceShape.OnPenChanged)));

      private IScreenSpaceScaler m_ScreenSpaceScaler = null;

      // Methods
      protected ScreenSpaceShape()
      {
      }

      protected override void OnVisualParentChanged(DependencyObject oldParent)
      {
         base.OnVisualParentChanged(oldParent);
         m_ScreenSpaceScaler = null;
      }

      protected override Size ArrangeOverride(Size finalSize)
      {
         Size size;
         Stretch mode = this.Stretch;
         if (mode == Stretch.None)
         {
            //StretchMatrixField.ClearValue(this);
            StretchMatrix = null;
            this.ResetRenderedGeometry();
            size = finalSize;
         }
         else
         {
            size = this.GetStretchedRenderSizeAndSetStretchMatrix(mode, this.GetStrokeThickness(), finalSize, this.GetDefiningGeometryBounds());
         }
         if (this.SizeIsInvalidOrEmpty(size))
         {
            size = new Size(0.0, 0.0);
            this._renderedGeometry = Geometry.Empty;
         }
         return size;
      }

      internal virtual void CacheDefiningGeometry()
      {
      }

      internal void EnsureRenderedGeometry()
      {
         if (this._renderedGeometry == null)
         {
            this._renderedGeometry = this.DefiningGeometry;
            if (this.Stretch != Stretch.None)
            {
               Geometry objB = this._renderedGeometry.CloneCurrentValue();
               if (object.ReferenceEquals(this._renderedGeometry, objB))
               {
                  this._renderedGeometry = objB.Clone();
               }
               else
               {
                  this._renderedGeometry = objB;
               }
               Transform transform = this._renderedGeometry.Transform;
               //BoxedMatrix matrix = StretchMatrixField.GetValue(this);
               BoxedMatrix matrix = StretchMatrix;
               Matrix matrix2 = (matrix == null) ? Matrix.Identity : matrix.Value;
               //if ((transform == null) || transform.IsIdentity)
               if ((transform == null) || transform == Transform.Identity)
               {
                  this._renderedGeometry.Transform = new MatrixTransform(matrix2);
               }
               else
               {
                  this._renderedGeometry.Transform = new MatrixTransform(transform.Value * matrix2);
               }
            }
         }
      }

      internal virtual Rect GetDefiningGeometryBounds()
      {
         return this.DefiningGeometry.Bounds;
      }

      internal virtual Size GetNaturalSize()
      {
         Geometry definingGeometry = this.DefiningGeometry;
         Pen pen = this.GetPen();
         DashStyle dashStyle = null;
         if (pen != null)
         {
            dashStyle = pen.DashStyle;
            if (dashStyle != null)
            {
               pen.DashStyle = null;
            }
         }
         Rect renderBounds = definingGeometry.GetRenderBounds(pen);
         if (dashStyle != null)
         {
            pen.DashStyle = dashStyle;
         }
         return new Size(Math.Max(renderBounds.Right, 0.0), Math.Max(renderBounds.Bottom, 0.0));
      }

      private IScreenSpaceScaler GetScreenSpaceScaler()
      {
         var p = VisualTreeHelper.GetParent(this);
         while (p != null && !(p is IScreenSpaceScaler))
         {
            p = VisualTreeHelper.GetParent(p);
         }
         return p as IScreenSpaceScaler;
      }

      internal double GetScaleFactor()
      {
         if (m_ScreenSpaceScaler == null)
         {
            m_ScreenSpaceScaler = GetScreenSpaceScaler();
            if (m_ScreenSpaceScaler != null)
            {
               m_ScreenSpaceScaler.ScaleFactorChanged += ScreenSpaceScaler_ScaleFactorChanged;
            }
         }
         double screenScaleFactor = 1.0;
         if (m_ScreenSpaceScaler != null && m_ScreenSpaceScaler.ScaleFactor > 0.0)
         {
            screenScaleFactor = m_ScreenSpaceScaler.ScaleFactor;
         }
         return screenScaleFactor;
      }

      internal Pen GetPen()
      {
         if (this.IsPenNoOp)
         {
            return null;
         }
         if (this._pen == null)
         {
            
            //bool flag;
            double num = 0.0;
            num = Math.Abs(this.StrokeThickness);
            var screenScaleFactor = GetScaleFactor();
            this._pen = new Pen();
            //this._pen.CanBeInheritanceContext = false;
            this._pen.Thickness = num / screenScaleFactor;
            this._pen.Brush = this.Stroke;
            this._pen.StartLineCap = this.StrokeStartLineCap;
            this._pen.EndLineCap = this.StrokeEndLineCap;
            this._pen.DashCap = this.StrokeDashCap;
            this._pen.LineJoin = this.StrokeLineJoin;
            this._pen.MiterLimit = this.StrokeMiterLimit / screenScaleFactor;
            DoubleCollection dashes = null;
            if (this.StrokeDashArray != null)
            {
               dashes = new DoubleCollection();
               foreach(var d in this.StrokeDashArray)
               {
                  dashes.Add(d);
               }
            }
            double strokeDashOffset = this.StrokeDashOffset;
            if ((dashes != null) || (strokeDashOffset != 0.0))
            {
               this._pen.DashStyle = new DashStyle(dashes, strokeDashOffset);
            }
         }
         return this._pen;
      }

      private void ScreenSpaceScaler_ScaleFactorChanged(object sender, EventArgs e)
      {
         _pen = null;
         InvalidateVisual();
         InvalidateMeasure();
      }

      internal Size GetStretchedRenderSize(Stretch mode, double strokeThickness, Size availableSize, Rect geometryBounds)
      {
         double num;
         double num2;
         double num3;
         double num4;
         Size size;
         this.GetStretchMetrics(mode, strokeThickness, availableSize, geometryBounds, out num, out num2, out num3, out num4, out size);
         return size;
      }

      internal Size GetStretchedRenderSizeAndSetStretchMatrix(Stretch mode, double strokeThickness, Size availableSize, Rect geometryBounds)
      {
         double num;
         double num2;
         double num3;
         double num4;
         Size size;
         this.GetStretchMetrics(mode, strokeThickness, availableSize, geometryBounds, out num, out num2, out num3, out num4, out size);
         Matrix identity = Matrix.Identity;
         identity.ScaleAt(num, num2, geometryBounds.Location.X, geometryBounds.Location.Y);
         identity.Translate(num3, num4);
         //StretchMatrixField.SetValue(this, new BoxedMatrix(identity));
         StretchMatrix.Value = identity;
         this.ResetRenderedGeometry();
         return size;
      }

      internal void GetStretchMetrics(Stretch mode, double strokeThickness, Size availableSize, Rect geometryBounds, out double xScale, out double yScale, out double dX, out double dY, out Size stretchedSize)
      {
         if (!geometryBounds.IsEmpty)
         {
            double num = strokeThickness / 2.0;
            bool flag = false;
            xScale = Math.Max((double)(availableSize.Width - strokeThickness), (double)0.0);
            yScale = Math.Max((double)(availableSize.Height - strokeThickness), (double)0.0);
            dX = num - geometryBounds.Left;
            dY = num - geometryBounds.Top;
            if (geometryBounds.Width > (xScale * double.Epsilon))
            {
               xScale /= geometryBounds.Width;
            }
            else
            {
               xScale = 1.0;
               flag = true;
            }
            if (geometryBounds.Height > (yScale * double.Epsilon))
            {
               yScale /= geometryBounds.Height;
            }
            else
            {
               yScale = 1.0;
               flag = true;
            }
            if ((mode != Stretch.Fill) && !flag)
            {
               if (mode == Stretch.Uniform)
               {
                  if (yScale > xScale)
                  {
                     yScale = xScale;
                  }
                  else
                  {
                     xScale = yScale;
                  }
               }
               else if (xScale > yScale)
               {
                  yScale = xScale;
               }
               else
               {
                  xScale = yScale;
               }
            }
            stretchedSize = new Size((geometryBounds.Width * xScale) + strokeThickness, (geometryBounds.Height * yScale) + strokeThickness);
         }
         else
         {
            xScale = yScale = 1.0;
            dX = dY = 0.0;
            stretchedSize = new Size(0.0, 0.0);
         }
      }

      internal double GetStrokeThickness()
      {
         if (this.IsPenNoOp)
         {
            return 0.0;
         }
         return Math.Abs(this.StrokeThickness / GetScaleFactor());
      }

      /*private struct NanUnion
      {
         // Fields
         internal double DoubleValue;
         internal ulong UintValue;
      }*/

      public static bool IsNaN(double value)
      {
         /*NanUnion union = new NanUnion();
         union.DoubleValue = value;
         ulong num = union.UintValue & 18442240474082181120L;
         ulong num2 = union.UintValue & ((ulong)0xfffffffffffffL);
         if ((num != 0x7ff0000000000000L) && (num != 18442240474082181120L))
         {
            return false;
         }
         return (num2 != 0L);*/
         return Double.IsNaN(value);
      }

      internal static bool IsDoubleFinite(object o)
      {
         double d = (double)o;
         return (!double.IsInfinity(d) && !IsNaN(d));
      }

      internal static bool IsDoubleFiniteNonNegative(object o)
      {
         double d = (double)o;
         return ((!double.IsInfinity(d) && !IsNaN(d)) && (d >= 0.0));
      }

      internal static bool IsDoubleFiniteOrNaN(object o)
      {
         double d = (double)o;
         return !double.IsInfinity(d);
      }

      protected override Size MeasureOverride(Size constraint)
      {
         Size naturalSize;
         this.CacheDefiningGeometry();
         Stretch mode = this.Stretch;
         if (mode == Stretch.None)
         {
            naturalSize = this.GetNaturalSize();
         }
         else
         {
            naturalSize = this.GetStretchedRenderSize(mode, this.GetStrokeThickness(), constraint, this.GetDefiningGeometryBounds());
         }
         if (this.SizeIsInvalidOrEmpty(naturalSize))
         {
            naturalSize = new Size(0.0, 0.0);
            this._renderedGeometry = Geometry.Empty;
         }
         return naturalSize;
      }

      private static void OnPenChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
      {
         ((ScreenSpaceShape)d)._pen = null;
      }

      protected override void OnRender(DrawingContext drawingContext)
      {
         this.EnsureRenderedGeometry();
         if (this._renderedGeometry != Geometry.Empty)
         {
            drawingContext.DrawGeometry(this.Fill, this.GetPen(), this._renderedGeometry);
         }
      }

      internal void ResetRenderedGeometry()
      {
         this._renderedGeometry = null;
      }

      internal bool SizeIsInvalidOrEmpty(Size size)
      {
         if (!IsNaN(size.Width) && !IsNaN(size.Height))
         {
            return size.IsEmpty;
         }
         return true;
      }

      // Properties
      protected abstract Geometry DefiningGeometry { get; }

      public Brush Fill
      {
         get
         {
            return (Brush)base.GetValue(FillProperty);
         }
         set
         {
            base.SetValue(FillProperty, value);
         }
      }

      public virtual Transform GeometryTransform
      {
         get
         {
            //BoxedMatrix matrix = StretchMatrixField.GetValue(this);
            BoxedMatrix matrix = StretchMatrix;
            if (matrix == null)
            {
               return Transform.Identity;
            }
            return new MatrixTransform(matrix.Value);
         }
      }

      public static bool IsZero(double value)
      {
         return (Math.Abs(value) < 2.2204460492503131E-15);
      }

      internal bool IsPenNoOp
      {
         get
         {
            double strokeThickness = this.StrokeThickness;
            if ((this.Stroke != null) && !IsNaN(strokeThickness))
            {
               return IsZero(strokeThickness);
            }
            return true;
         }
      }

      public virtual Geometry RenderedGeometry
      {
         get
         {
            this.EnsureRenderedGeometry();
            Geometry objA = this._renderedGeometry.CloneCurrentValue();
            if ((objA == null) || (objA == Geometry.Empty))
            {
               return Geometry.Empty;
            }
            if (object.ReferenceEquals(objA, this._renderedGeometry))
            {
               objA = objA.Clone();
               objA.Freeze();
            }
            return objA;
         }
      }

      public Stretch Stretch
      {
         get
         {
            return (Stretch)base.GetValue(StretchProperty);
         }
         set
         {
            base.SetValue(StretchProperty, value);
         }
      }

      public Brush Stroke
      {
         get
         {
            return (Brush)base.GetValue(StrokeProperty);
         }
         set
         {
            base.SetValue(StrokeProperty, value);
         }
      }

      public DoubleCollection StrokeDashArray
      {
         get
         {
            return (DoubleCollection)base.GetValue(StrokeDashArrayProperty);
         }
         set
         {
            base.SetValue(StrokeDashArrayProperty, value);
         }
      }

      public PenLineCap StrokeDashCap
      {
         get
         {
            return (PenLineCap)base.GetValue(StrokeDashCapProperty);
         }
         set
         {
            base.SetValue(StrokeDashCapProperty, value);
         }
      }

      public double StrokeDashOffset
      {
         get
         {
            return (double)base.GetValue(StrokeDashOffsetProperty);
         }
         set
         {
            base.SetValue(StrokeDashOffsetProperty, value);
         }
      }

      public PenLineCap StrokeEndLineCap
      {
         get
         {
            return (PenLineCap)base.GetValue(StrokeEndLineCapProperty);
         }
         set
         {
            base.SetValue(StrokeEndLineCapProperty, value);
         }
      }

      public PenLineJoin StrokeLineJoin
      {
         get
         {
            return (PenLineJoin)base.GetValue(StrokeLineJoinProperty);
         }
         set
         {
            base.SetValue(StrokeLineJoinProperty, value);
         }
      }

      public double StrokeMiterLimit
      {
         get
         {
            return (double)base.GetValue(StrokeMiterLimitProperty);
         }
         set
         {
            base.SetValue(StrokeMiterLimitProperty, value);
         }
      }

      public PenLineCap StrokeStartLineCap
      {
         get
         {
            return (PenLineCap)base.GetValue(StrokeStartLineCapProperty);
         }
         set
         {
            base.SetValue(StrokeStartLineCapProperty, value);
         }
      }

      [TypeConverter(typeof(LengthConverter))]
      public double StrokeThickness
      {
         get
         {
            return (double)base.GetValue(StrokeThicknessProperty);
         }
         set
         {
            base.SetValue(StrokeThicknessProperty, value);
         }
      }
   }
}
