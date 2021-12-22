using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DS.RVT.RibbonForTest
{
    class Button
    {
        public static PushButtonData button1;
        public static PushButtonData button2;
        public static PushButtonData button3;

        string ButtonPath;
        string ButtonDescription;

        public void AddButtons()
        {
            //Create button1
            string button1Name = "DS.PipesCollisionsElliminator";
            AddButton1(button1Name);

            //Create button2
            string button2Name = "DS.RVT.PipeTest";
            AddButton2(button2Name);

            //Create button3
            string button3Name = "DS.RVT.WaveAlgorythm";
            AddButton3(button3Name);
        }

        void AddButton1(string buttonName)
        {
            AssignProperties(buttonName);

            // Create push button
            button1 = new PushButtonData(buttonName, buttonName, ButtonPath, buttonName + ".ExternalCommand")
            {
                ToolTip = ButtonDescription
            };
        }

        void AddButton2(string buttonName)
        {
            AssignProperties(buttonName);

            // Create push button
            button2 = new PushButtonData(buttonName, buttonName, ButtonPath, buttonName + ".ExternalCommand")
            {
                ToolTip = ButtonDescription
            };
        }


        void AddButton3(string buttonName)
        {
            AssignProperties(buttonName);

            // Create push button
            button3 = new PushButtonData(buttonName, buttonName, ButtonPath, buttonName + ".ExternalCommand")
            {
                ToolTip = ButtonDescription
            };
        }

        void AssignProperties(string buttonName)
        {
            string path = String.Format(@"%AppData%\Autodesk\Revit\Addins\2020\{0}\{0}.dll", buttonName);
            ButtonPath = Environment.ExpandEnvironmentVariables(path);

            Assembly assembly = Assembly.LoadFrom(ButtonPath);

            var descriptionAttribute1 = assembly.GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false).
                OfType<AssemblyDescriptionAttribute>().FirstOrDefault();

            System.Diagnostics.FileVersionInfo fvi = System.Diagnostics.FileVersionInfo.GetVersionInfo(assembly.Location);
            string version = fvi.FileVersion;

            ButtonDescription = descriptionAttribute1.Description + "\nProduct version: " + version;
        }
    }
}
