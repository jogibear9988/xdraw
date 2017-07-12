using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace XDraw
{
   public interface IDockManager
   {
      IDockDocument FindDocument(string id);

      IDockDocument ShowDocument(string id, CreateDockDocumentDelegate createDocCallback);

      IDockDocument ActiveDocument { get; set; }

      //string GetDocumentName(IDockDocument doc);

      IList<IDockDocument> Documents { get; }

      void CloseDocument(IDockDocument doc);

      void CloseDocument(string id);

      IDockDocument ShowDrawing(IXDrawDrawing drawing);

      IDockDocument OpenFile(string path);

      /*IDockPane FindPane(string id);

      IDockPane ShowPane(string id, CreateDockPaneDelegate createDockPane);

      void ClosePane(IDockPane pane);

      void ClosePane(string id);

      string GetPaneName(IDockPane pane);

      IEnumerable<IDockPane> Panes { get; }*/
   }
}
