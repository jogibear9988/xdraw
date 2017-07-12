using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XDraw.Editor
{
   /// <summary>
   /// Possible workspace size modes for XDraw editor
   /// </summary>
   public enum WorkspaceSizeMode
   {
      /// <summary>
      /// Workspace size is fixed
      /// </summary>
      Fixed,

      /// <summary>
      /// Workspace size can be adjusted by Drag&Drop
      /// </summary>
      Manual,

      /// <summary>
      /// Workspace size ist automatically adjusted to content
      /// </summary>
      Auto
   }

   /// <summary>
   /// Possible selection modes for XDraw editor
   /// </summary>
   [Flags]
   public enum SelectionModes
   {
      /// <summary>
      /// No selection possible
      /// </summary>
      None = 0,

      /// <summary>
      /// Select by clicking
      /// </summary>
      Click = 0x01,

      /// <summary>
      /// Select by draging a rectangle
      /// </summary>
      /// <remarks>
      /// This selection mode can not be combined with Lasso selection mode.
      /// </remarks>
      Rectangle = 0x02,

      /// <summary>
      /// Select by draging a PolyLine
      /// </summary>
      /// <remarks>
      /// This selection mode can not be combined with Rectangle selection mode.
      /// </remarks>
      Lasso = 0x04,

      /// <summary>
      /// Intersection of Rectangle or Lasso selects already
      /// </summary>
      IntercesctionSelects = 0x80,

      /// <summary>
      /// Select by draging a rectangle or clicking
      /// </summary>
      /// <remarks>
      /// This selection mode can not be combined with Lasso selection mode.
      /// </remarks>
      RectangleClick = Rectangle | Click,

      /// <summary>
      /// Select by draging a PolyLine or clicking
      /// </summary>
      /// <remarks>
      /// This selection mode can not be combined with Rectangle selection mode.
      /// </remarks>
      LassoClick = Lasso | Click
   }

   public enum Tool
   {
      Select,
      NewObject
   }

   /// <summary>
   /// Possible Drag&Drop operations for XDraw editor
   /// </summary>
   /// <remarks>We might not need this !?</remarks>
   public enum DragMode
   {
      /// <summary>
      /// No Drag&Drop
      /// </summary>
      None,

      /// <summary>
      /// Creating new object
      /// </summary>
      NewObject,

      /// <summary>
      /// Move object
      /// </summary>
      Move,

      /// <summary>
      /// Size at left side of object
      /// </summary>
      SizeLeft,

      /// <summary>
      /// 
      /// </summary>
      SizeRight,
      SizeTop,
      SizeBottom,
      SizeTopLeft,
      SizeTopRight,
      SizeBottomLeft,
      SizeBottomRight
   }
}
