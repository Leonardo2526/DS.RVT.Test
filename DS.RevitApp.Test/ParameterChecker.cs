using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Rhino.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DS.RevitApp.Test
{
    internal class ParameterChecker
    {
        public static string ParamName1 = "OLP_MEPAC_Add";
        private readonly Document _doc;

        private static readonly List<BuiltInCategory> _cats = new()
            {
                { BuiltInCategory.OST_PipeCurves },
                { BuiltInCategory.OST_DuctCurves },
                { BuiltInCategory.OST_CableTray }
            };

        public ParameterChecker(Document doc)
        {
            _doc = doc;
        }

        public void Check() 
        {
            CheckParameterExist(_doc, ParamName1);
        }

        private void CheckParameterExist(Document doc, string parameterName)
        {
            try
            {
                var uidoc = new UIDocument(doc);
                var app = uidoc.Application.Application;
                var fop = app.SharedParametersFilename;
                app.SharedParametersFilename = @"\\DISKSTATION\Производство\Ревит\REVIT_SETUP\06_Файл_общих_параметров\OLP_ФОП_2019.txt";

                var parameters = GetSharedParameters(doc);
                var paramter = parameters.FirstOrDefault(x => x.Key.Name == parameterName).Key;
                var cats = new CategorySet();
                _cats.Select(x => Category.GetCategory(doc, x)).ToList().ForEach(x => cats.Insert(x));
                var binding = app.Create.NewInstanceBinding(cats);
                if (paramter == null)
                {
                    using (var tr = new Transaction(doc, "Insert parameters"))
                    {
                        tr.Start();

                        var file = app.OpenSharedParameterFile();
                        AddParameter(doc, binding, file, parameterName);
                        MakeVaryByGroup(doc, parameterName);

                        tr.Commit();
                    }
                }
                app.SharedParametersFilename = fop;
            }
            catch (Exception ex)
            {
            }

        }

        /// <summary>
        /// Делает параметр изменяемым по группам
        /// </summary>
        /// <param name="doc">Документ</param> 
        /// <param name="name">Имя параметра</param>
        private static void MakeVaryByGroup(Document doc, string name)
        {
            var bindings = doc.ParameterBindings;
            var iter = bindings.ForwardIterator();
            while (iter.MoveNext())
            {
                var def = iter.Key;
                if (def.Name == name)
                {
                    var internalDef = def as InternalDefinition;
                    if (internalDef != null)
                    {
                        var result = internalDef.SetAllowVaryBetweenGroups(doc, true);
                    }
                }
            }
        }

        /// <summary>
        /// Добавляет общий параметр проекта
        /// </summary>
        /// <param name="doc">Документ</param>
        /// <param name="binding">Привязки</param>
        /// <param name="file">Файл общих параметров</param>
        /// <param name="name">Имя параметра для добавления</param>
        private static void AddParameter(Document doc, InstanceBinding binding, DefinitionFile file, string name)
        {
            try
            {
                Definition def = null;
                foreach (var dg in file.Groups)
                {
                    foreach (var d in dg.Definitions)
                    {
                        if (d.Name == name)
                            def = d;
                    }
                }

                doc.ParameterBindings.Insert(def, binding, BuiltInParameterGroup.PG_ANALYSIS_RESULTS);
            }
            catch (Exception ex)
            {
            }
        }

        /// <summary>
        /// Получаесят словать параметров и id категорий к которым он применен
        /// </summary>
        private static Dictionary<Definition, List<Category>> GetSharedParameters(Document doc)
        {
            var parameters = new Dictionary<Definition, List<Category>>();
            var bindings = doc.ParameterBindings;
            var iter = bindings.ForwardIterator();
            while (iter.MoveNext())
            {
                var def = iter.Key;
                var binding = (ElementBinding)iter.Current;
                parameters.Add(def, binding.Categories.Cast<Category>().ToList());
            }
            return parameters;
        }

    }
}
