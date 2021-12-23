using Autodesk.Revit.DB;
using System.Collections.Generic;

namespace DS.RVT.ModelSpaceFragmentation
{


    class ModelSolid
    {
        readonly Document Doc;

        public ModelSolid(Document doc)
        {
            Doc = doc;
        }

        ElementUtils ElemUtils = new ElementUtils();

        public Dictionary<Element, List<Solid>> modelSolids = new Dictionary<Element, List<Solid>>();

        public void Get()
        {
            FilteredElementCollector collector = new FilteredElementCollector(Doc);

            ICollection<BuiltInCategory> elementCategoryFilters = new List<BuiltInCategory>
                {
                    BuiltInCategory.OST_PipeCurves,
                    BuiltInCategory.OST_Walls
                };

            ElementMulticategoryFilter elementMulticategoryFilter = new ElementMulticategoryFilter(elementCategoryFilters);

            IList<Element> intersectedElementsBox = collector.WherePasses(elementMulticategoryFilter).ToElements();

            List<Solid> solids = new List<Solid>();
            foreach (Element elem in intersectedElementsBox)
            {
                solids = ElemUtils.GetSolids(elem);
                modelSolids.Add(elem, solids);
            }
        }


    }
}
