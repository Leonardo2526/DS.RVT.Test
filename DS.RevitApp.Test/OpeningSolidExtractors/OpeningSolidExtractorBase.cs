using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test.Energy
{
    internal abstract class OpeningSolidExtractorBase<THost>
    {
        protected readonly THost _hostElement;
        protected readonly Document _activeDoc;
        protected readonly IEnumerable<RevitLinkInstance> _links;

        public OpeningSolidExtractorBase(THost hostElement, Document activeDoc, IEnumerable<RevitLinkInstance> links = null)
        {
            _hostElement = hostElement;
            _activeDoc = activeDoc;
            _links = links;
        }

        public abstract Solid GetSolid(Opening opening);

    }
}
