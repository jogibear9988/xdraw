using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Media;

namespace XDraw
{
   public interface IDockDocument
   {
      object DocumentContext { get; }

      string DocumentId { get; }

      object DocumentContent { get; }

      string DocumentTitle { get; }

      string DocumentToolTip { get; }

      ImageSource DocumentIcon { get; }

      IInputElement GetDocumentDefaultElement();

      void DocumentClosing(ref bool cancel);

      void DocumentClosed();

      bool IsDocumentModified { get; }

      bool IsDocumentLocked { get; }
   }

   public interface ISaveableDockDocument : IDockDocument
   {
      bool CanSaveDocument { get; }

      void SaveDocument();
   }

   public interface IToolboxItemsSource
   {
      IEnumerable<INewItemType> ToolboxItems { get; }
   }
}
