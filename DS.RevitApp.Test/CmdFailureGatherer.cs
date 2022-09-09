﻿using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
    /// <summary>
    /// Collect all failure message description strings.
    /// </summary>
    class MessageDescriptionGatheringPreprocessor : IFailuresPreprocessor
    {
        List<string> FailureList { get; set; }

        public MessageDescriptionGatheringPreprocessor()
        {
            FailureList = new List<string>();
        }

        public FailureProcessingResult PreprocessFailures(
          FailuresAccessor failuresAccessor)
        {
            foreach (FailureMessageAccessor fMA
              in failuresAccessor.GetFailureMessages())
            {
                FailureList.Add(fMA.GetDescriptionText());
                FailureDefinitionId FailDefID
                  = fMA.GetFailureDefinitionId();

                //if (FailDefID == BuiltInFailures
                //  .GeneralFailures.DuplicateValue)
                //    failuresAccessor.DeleteWarning(fMA);
            }
            return FailureProcessingResult.Continue;
        }

        public void ShowDialogue()
        {
            string s = string.Join("\r\n", FailureList);

            TaskDialog.Show("Post Processing Failures:", s);
        }
    }

    [Transaction(TransactionMode.Manual)]
    class CmdFailureGatherer : IExternalCommand
    {
        public Result Execute(
            ExternalCommandData commandData,
            ref string message,
            ElementSet elements)
        {
            UIApplication uiApp = commandData.Application;
            Document activeDoc = uiApp.ActiveUIDocument.Document;

            MessageDescriptionGatheringPreprocessor pp
              = new MessageDescriptionGatheringPreprocessor();

            using (Transaction t = new Transaction(activeDoc))
            {
                FailureHandlingOptions ops
                  = t.GetFailureHandlingOptions();

                ops.SetFailuresPreprocessor(pp);
                t.SetFailureHandlingOptions(ops);

                t.Start("Marks");

                IList<Element> specEqu
                  = new FilteredElementCollector(activeDoc)
                    .OfCategory(BuiltInCategory.OST_SpecialityEquipment)
                    .WhereElementIsNotElementType()
                    .ToElements();

                if (specEqu.Count >= 2)
                {
                    for (int i = 0; i < 2; i++)
                        specEqu[i].get_Parameter(
                          BuiltInParameter.ALL_MODEL_MARK).Set(
                            "Duplicate Mark");
                }
                t.Commit();
            }
            pp.ShowDialogue();

            return Result.Succeeded;
        }
    }
}
