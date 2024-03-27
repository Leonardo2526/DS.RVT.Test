using Autodesk.Revit.DB;
using OLMP.RevitAPI.Tools;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitCmd.EnergyTest
{
    internal static class DocFilterBuilder
    {
        public static DocumentFilter GetFilter(Document activeDoc,
          IEnumerable<RevitLinkInstance> allLoadedLinks)
        {
            //create global filter
            var docs = new List<Document>() { activeDoc };
            var excludedCategories = new List<BuiltInCategory>()
        {
            BuiltInCategory.OST_GenericAnnotation,
            BuiltInCategory.OST_TelephoneDevices,
            BuiltInCategory.OST_Materials,
            //BuiltInCategory.OST_GenericModel,
            BuiltInCategory.OST_Massing
        };
            var globalFilter = new DocumentFilter(docs, activeDoc, allLoadedLinks);
            globalFilter.QuickFilters =
            [
                (new ElementMulticategoryFilter(excludedCategories, true), null),
                (new ElementIsElementTypeFilter(true), null),
            ];
            return globalFilter;
        }
    }
}
