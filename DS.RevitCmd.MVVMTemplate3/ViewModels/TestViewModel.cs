using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.RevitLib.Utils;
using Revit.Async;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Input;

namespace DS.RevitCmd.MVVMTemplate3
{
    public class TestViewModel
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;
        //private RevitTask _revitTask = new RevitTask();

        public TestViewModel(UIDocument uiDoc)
        {
            _uiDoc = uiDoc;
            _doc = uiDoc.Document;
        }

        public ICommand Run => new RelayCommand(async p =>
        {
            await RevitTask.RunAsync(() =>
            {
                TaskDialog.Show("revt", "start");
                Debug.WriteLine("Start executing!");
                var transactionBuilder = new TransactionBuilder(_doc);
                transactionBuilder.Build(() => _doc.Regenerate(), "regen");
                Debug.WriteLine("Executed!");
            });
            //await RevitTask.RunAsync(() => Debug.WriteLine("Tran"));
        });
    }
}
