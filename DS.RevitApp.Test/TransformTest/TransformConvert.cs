﻿using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using DS.RevitLib.Utils.Solids.Models;
using OLMP.RevitLib.MEPAC.Collisons.Resolvers.MEPBypass.ElementsTransfer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.TransformTest
{
    internal class TransformConvert
    {
        UIDocument uidoc;
        Document doc;

        public TransformConvert(UIDocument uidoc, Document doc)
        {
            this.uidoc = uidoc;
            this.doc = doc;
        }

        public void Run()
        {
            Reference reference = uidoc.Selection.PickObject(ObjectType.Element, "Select element 1");
            Element element = doc.GetElement(reference);
            var model1 = new SolidModelExt(element);

            reference = uidoc.Selection.PickObject(ObjectType.Element, "Select element 2");
            element = doc.GetElement(reference);
            var model2 = new SolidModelExt(element);

            var builder = new BasisTransformBuilder(model1.Basis.Clone(), model2.Basis.Clone());
            var trModel = builder.Build();
            var tr1 = trModel.Transforms.First();
            var b = tr1.get_Basis(0);           
        }
    }
}
