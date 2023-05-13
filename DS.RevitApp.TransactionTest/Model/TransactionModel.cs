using Autodesk.Revit.DB;
using Autodesk.Revit.DB.Plumbing;
using Autodesk.Revit.UI;
using DS.ClassLib.VarUtils;
using DS.ClassLib.VarUtils.Events;
using DS.RevitLib.Utils;
using DS.RevitLib.Utils.Elements;
using DS.RevitLib.Utils.Extensions;
using DS.RevitLib.Utils.MEP;
using DS.RevitLib.Utils.MEP.Creator;
using DS.RevitLib.Utils.MEP.SystemTree;
using DS.RevitLib.Utils.ModelCurveUtils;
using DS.RevitLib.Utils.TransactionCommitter;
using DS.RevitLib.Utils.Transactions;
using DS.RevitLib.Utils.Various;
using Revit.Async;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;

namespace DS.RevitApp.TransactionTest.Model
{
    public class TransactionModel
    {
        private readonly Document _doc;
        private readonly UIDocument _uiDoc;


        public TransactionModel(Document doc, UIDocument uiDoc)
        {
            _doc = doc;
            _uiDoc = uiDoc;

            //var mEPCurve = new ElementSelector(uiDoc).Pick() as MEPCurve;
            //MEPSystem = new SimpleMEPSystemBuilder(mEPCurve).Build();
            //_trb = new TransactionBuilder(doc, null, null, false);
            //_cl = new Class1();
        }

        public MEPSystemModel MEPSystem { get; private set; }

        private readonly TransactionBuilder _trb;
        private readonly Class1 _cl;

        //public event EventHandler<EventType> Event;

        public void Create(int offset = 0, string trName = "showLines")
        {
            var path = new List<XYZ>
            {
                new XYZ(0 + offset,0,0),
                new XYZ(5 + offset,0,0),
                new XYZ(5 + offset,5,0),
                new XYZ(10 + offset,5,0),
                new XYZ(10 + offset,0,0)
            };

            Debug.Print($"Transaction started");
            var trb = new TransactionBuilder(_doc);
            trb.Build(() => ShowLines(path), trName);
            Debug.Print($"Transaction executed");

            //trb.Build(() => ShowcCrves(path), "show curves");
        }

        public void CreateRevitTask()
        {
            var path = new List<XYZ>
            {
                new XYZ(0,0,0),
                new XYZ(5,0,0),
                new XYZ(5,5,0),
                new XYZ(10,5,0),
                new XYZ(10,0,0)
            };

            RevitTask.RunAsync(() =>
            {
                var trb = new TransactionBuilder(_doc);
                trb.Build(() => ShowLines(path), "show lines");
                //trb.Build(() => ShowcCrves(path), "show curves");
            });
        }

        private void ShowLines(List<XYZ> path)
        {
            var mcreator = new ModelCurveCreator(_doc);
            for (int i = 0; i < path.Count - 1; i++)
            {
                mcreator.Create(path[i], path[i + 1]);
            }
        }

        private void ShowCurves(List<XYZ> path)
        {
            Reference reference = _uiDoc.Selection.PickObject(Autodesk.Revit.UI.Selection.ObjectType.Element, "Select element");
            var mEPCurve = _doc.GetElement(reference) as MEPCurve;

            var builder = new BuilderByPoints(mEPCurve, path).BuildMEPCurves().WithElbows(mEPCurve);
        }


        public async Task DeleteElements()
        {
            var taskEvent = new TaskComplition(_cl);
            var trgBuilder = new TrgEventBuilder(_doc);

            await trgBuilder.BuildAsync(async () =>
            {
                await _trb.BuilAsync(() => MEPSystem.AllElements.ForEach(e => { _doc.Delete(e.Id); }), "delete");
            }, new CommitOnClose(), taskEvent, false, $"Commit");

            await _trb.BuilAsync(() => _doc.Regenerate(), "regen");
        }


        public async Task DeleteElements1()
        {
            using (var trg = new TransactionGroup(_doc, "trg"))
            {
                trg.Start();

                await _trb.BuilAsync(() => MEPSystem.AllElements.ForEach(e => { _doc.Delete(e.Id); }), "delete");

                trg.RollBack();
            }

            await _trb.BuilAsync(() => _doc.Regenerate(), "regen");
        }

        public async Task DeleteElementsAsync()
        {
            using (var trg = new TransactionGroup(_doc, "trg"))
            {
                trg.Start();

                var action = () =>
                {
                    using (var tr = new Transaction(_doc, "delete"))
                    {
                        tr.Start();
                        MEPSystem.AllElements.ForEach(e => { _doc.Delete(e.Id); });
                        tr.Commit();
                    }
                };

                await RevitTask.RunAsync(action);

                trg.RollBack();
            }

            await _trb.BuilAsync(() => _doc.Regenerate(), "regen");
        }

        public void DeleteElements2()
        {
            using (var trg = new TransactionGroup(_doc, "trg"))
            {
                trg.Start();

                using (var tr = new Transaction(_doc, "delete"))
                {
                    tr.Start();
                    MEPSystem.AllElements.ForEach(e => { _doc.Delete(e.Id); });
                    tr.Commit();
                }

                trg.RollBack();
            }

            using (var tr = new Transaction(_doc, "delete"))
            {
                tr.Start();
                _doc.Regenerate();
                tr.Commit();
            }

            //_uiDoc.RefreshActiveView();
        }

        public void DeleteElementsWithDisconnect(List<Element> elems)
        {
            using (var tr = new Transaction(_doc, "disconnect"))
            {
                tr.Start();
                elems.ForEach(e => MEPElementUtils.Disconnect(e));
                _doc.Regenerate();
                tr.Commit();
            }


            using (var trg = new TransactionGroup(_doc, "trg"))
            {
                trg.Start();

                using (var tr = new Transaction(_doc, "delete"))
                {
                    tr.Start();
                    elems.ForEach(e => { _doc.Delete(e.Id); });
                    tr.Commit();
                }

                trg.RollBack();
            }

            using (var tr_regen = new Transaction(_doc, "regen"))
            {
                tr_regen.Start();
                _doc.Regenerate();
                tr_regen.Commit();
            }


            elems = elems.Where(e => e.IsValidObject).ToList();
            using (var trc = new Transaction(_doc, "connect"))
            {
                trc.Start();
                foreach (var e in elems)
                {
                    var cons = elems.Where(obj => obj.Id != e.Id).ToList();
                    foreach (var cs in cons)
                    {
                        e.Connect(cs);
                    }
                }
                trc.Commit();
            }
        }

        public void DeleteElementsWtihSingleTransaction(List<Element> elems)
        {
            //var elems = MEPSystem.AllElements;
            //foreach (Element item in elems)
            //{
            //    Debug.WriteLineIf(item.IsValidObject, item.Id);
            //    using (var tr = new Transaction(_doc, "delete"))
            //    {
            //        tr.Start();
            //        _doc.Delete(item.Id);
            //        tr.RollBack();
            //    }
            //}

            using (var tr = new Transaction(_doc, "delete"))
            {
                tr.Start();
                //_doc.Delete(elems[0].Id);
                //_doc.Delete(elems[1].Id);
                //_doc.Delete(elems[2].Id);
                //_doc.Delete(elems[3].Id);
                //_doc.Delete(elems[4].Id);


                //while(elems.Count > 0)
                //{
                //    _doc.Delete(elems.First().Id);
                //    elems.RemoveAt(0);
                //}

                //_doc.Delete(elems.First().Id);
                //_doc.Delete(elems.Last().Id);
                //foreach (Element item in elems)
                //{
                //    _doc.Delete(item.Id);
                //}

                _doc.Regenerate();
                elems.ForEach(e =>
                {
                    _doc.Delete(e.Id);
                    _doc.Regenerate();
                });
                _doc.Regenerate();
                tr.Commit();
            }



            using (var tr_regen = new Transaction(_doc, "regen"))
            {
                tr_regen.Start();
                _doc.Regenerate();
                tr_regen.Commit();
            }
        }

        public void DeleteElementsWtihSingleTransactionAndDisconnect(List<Element> elems)
        {
            //var mEPCurve = _doc.GetElement(new ElementId(705201)) as MEPCurve;
            //var mEPCurve = elems.First(obj => obj is MEPCurve) as MEPCurve;
            //MEPSystem mEPSystem = mEPCurve.MEPSystem;
           
            Dictionary<Element, List<Element>> elemsDict = new Dictionary<Element, List<Element>>();
            Dictionary<Element, ElementId> elemsTypesDict = new Dictionary<Element, ElementId>();
            foreach (var e in elems)
            {
                var connectedElems = ConnectorUtils.GetConnectedElements(e);
                elemsDict.Add(e, connectedElems);

                MEPCurve mEPCurve1 = e as MEPCurve;
                MEPSystem mEPSystem1 = mEPCurve1?.MEPSystem;
                if (mEPCurve1 != null && mEPSystem1 != null)
                {
                    Pipe pipe = e as Pipe;
                    if (pipe != null)
                    {
                        var id = mEPSystem1.GetTypeId();
                        elemsTypesDict.Add(e, id);
                    }
                }
            }

            using (var tr = new Transaction(_doc, "disconnect"))
            {
                tr.Start();
                elems.ForEach(e => MEPElementUtils.Disconnect(e));
                _doc.Regenerate();
                tr.Commit();
            }


            using (var tr = new Transaction(_doc, "delete"))
            {
                tr.Start();
                elems.ForEach(e => { _doc.Delete(e.Id); });
                tr.RollBack();
            }

            using (var tr_regen = new Transaction(_doc, "regen"))
            {
                tr_regen.Start();
                _doc.Regenerate();
                tr_regen.Commit();
            }


            //elems = elems.Where(e => e.IsValidObject).ToList();

            //using (var trc = new Transaction(_doc, "restoreSystem"))
            //{
            //    trc.Start();
            //    _doc.Regenerate();
            //    //foreach (var item in elemsTypesDict)
            //    //{
            //    //    Pipe pipe = item.Key as Pipe;
            //    //    MEPSystem s = pipe.MEPSystem;
            //    //    s.ChangeTypeId(item.Value);

            //    //}
            //    foreach (var item in elemsTypesDict)
            //    {
            //        Pipe pipe = item.Key as Pipe;
            //        pipe.SetSystemType(item.Value);
            //        //_doc.Regenerate();

            //        //MEPSystem s = pipe.MEPSystem;
            //        //s.ChangeTypeId(item.Value);

            //    }
            //    _doc.Regenerate();
            //    trc.Commit();
            //}

            //using (var trc = new Transaction(_doc, "connect"))
            //{
            //    trc.Start();
            //    foreach (var e in elems)
            //    {
            //        var cons = elemsDict.TryGetValue(e, out var t);
            //        foreach (var cs in t)
            //        { e.Connect(cs); }

            //    }
            //    _doc.Regenerate();
            //    trc.Commit();
            //}


            ////using (var trc = new Transaction(_doc, "restoreSystemFaminst"))
            //{
            //    trc.Start();
            //    _doc.Regenerate();

            //    var fam = elems.FirstOrDefault(e => e is FamilyInstance);
            //    var id = elemsTypesDict.First().Value;
            //    //var id = mEPSystem.GetTypeId();
            //    var conSet = ConnectorUtils.GetConnectorSet(fam);
            //    var cons = ConnectorUtils.GetConnectors(fam);



            //    fam.ChangeTypeId(id);

            //    _doc.Regenerate();
            //    trc.Commit();
            //}

            //elems = elems.Where(e => e.IsValidObject).ToList();

            //var selems = MEPElementUtils.GetSystemElements(_doc, mEPSystem.Name);
        }

        public void DeleteElements3()
        {
            using (var trg = new Transaction(_doc, "trg"))
            {
                trg.Start();

                using (var tr = new SubTransaction(_doc))
                {
                    tr.Start();
                    MEPSystem.AllElements.ForEach(e => { _doc.Delete(e.Id); });
                    tr.RollBack();
                }

                trg.RollBack();
            }

            using (var tr = new Transaction(_doc, "delete"))
            {
                tr.Start();
                _doc.Regenerate();
                tr.Commit();
            }

            //_uiDoc.RefreshActiveView();
        }

        public void Backward()
        {
            _cl.Backward();
        }

        public void Apply()
        {
            _cl.Apply();
        }

        public void RollBack()
        {
            _cl.RollBack();
        }

        public void Close()
        {
            _cl.Close();
        }

       
    }
}
