using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Reflection;
using System.Windows.Media;

namespace remes.XDraw
{
   public static class PathGeometryExtensions
   {
      public static Point GetFirstStartPoint(this PathGeometry path)
      {
         if (path.Figures.Count > 0)
         {
            return path.Figures[0].StartPoint;
         }
         return new Point();
      }

      public static IEnumerable<Point> GetAllPoints(this PathGeometry path)
      {
         foreach (var f in path.Figures)
         {
            yield return f.StartPoint;
            foreach (var s in f.Segments)
            {
               foreach (FieldInfo fi in s.GetType().GetFields())
               {
                  if (fi.FieldType == typeof(DependencyProperty))
                  {
                     var dp = fi.GetValue(null) as DependencyProperty;
                     if (dp.PropertyType == typeof(Point))
                     {
                        yield return (Point)s.GetValue(dp);
                     }
                  }
               }
            }
         }
      }

      public static void DoForAllPoints(this PathGeometry path, RefPointCallback callback)
      {
         foreach (var f in path.Figures)
         {
            var pt = f.StartPoint;
            callback(ref pt);
            f.StartPoint = pt;
            foreach (var s in f.Segments)
            {
               foreach (FieldInfo fi in s.GetType().GetFields())
               {
                  if (fi.FieldType == typeof(DependencyProperty))
                  {
                     var dp = fi.GetValue(null) as DependencyProperty;
                     if (dp.PropertyType == typeof(Point))
                     {
                        pt = (Point)s.GetValue(dp);
                        callback(ref pt);
                        s.SetValue(dp, pt);
                     }
                  }
               }
            }
         }
      }

      public static void DoForAllPointDPs(this PathGeometry path, DependencyPropertyCallback callback)
      {
         foreach (var f in path.Figures)
         {
            var t1 = f.GetType();
            var fii = t1.GetField("StartPointProperty");
            var oo = fii.GetValue(null);
            callback(f, oo as DependencyProperty);
            foreach (var s in f.Segments)
            {
               foreach (FieldInfo fi in s.GetType().GetFields())
               {
                  if (fi.FieldType == typeof(DependencyProperty))
                  {
                     var dp = fi.GetValue(null) as DependencyProperty;
                     if (dp.PropertyType == typeof(Point))
                     {
                        callback(s, dp);
                     }
                  }
               }
            }
         }
      }
   }

   public static class PathSegmentExtensions
   {
      public static void DoForAllPoints(this PathSegment segment, RefPointCallback callback)
      {
         foreach (FieldInfo fi in segment.GetType().GetFields())
         {
            if (fi.FieldType == typeof(DependencyProperty))
            {
               var dp = fi.GetValue(null) as DependencyProperty;
               if (dp.PropertyType == typeof(Point))
               {
                  var pt = (Point)segment.GetValue(dp);
                  callback(ref pt);
                  segment.SetValue(dp, pt);
               }
            }
         }
      }

      public static void DoForAllPointDPs(this PathSegment segment, DependencyPropertyCallback callback)
      {
         foreach (FieldInfo fi in segment.GetType().GetFields())
         {
            if (fi.FieldType == typeof(DependencyProperty))
            {
               var dp = fi.GetValue(null) as DependencyProperty;
               if (dp.PropertyType == typeof(Point))
               {
                  callback(segment, dp);
               }
            }
         }
      }
   }

   public delegate void RefPointCallback(ref Point pt);

   public delegate void DependencyPropertyCallback(DependencyObject obj, DependencyProperty dp);
}
