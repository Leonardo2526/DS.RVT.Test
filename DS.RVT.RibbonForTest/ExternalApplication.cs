using Autodesk.Revit.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace DS.RVT.RibbonForTest
{
    public class ExternalApplication : IExternalApplication
    {
        // class instance
        internal static ExternalApplication thisApp = null;

        public Result OnShutdown(UIControlledApplication application)
        {
            return Result.Succeeded;
        }

        public Result OnStartup(UIControlledApplication application)
        {
            Button button = new Button();
            button.AddButtons();

            // Create a custom ribbon tab  
            string tabName = "DS.RVT.Test";
            application.CreateRibbonTab(tabName); 

            // Create a ribbon panel 
            RibbonPanel m_projectPanel_1 = application.CreateRibbonPanel(tabName, "Tools");


            // Add the button to the panel
            // RibbonItem ribbonItem = m_projectPanel_1.AddItem(Button.button1);
            List<RibbonItem> projectButtons = new List<RibbonItem>();

            // Add the buttons to the panels
            projectButtons.AddRange(m_projectPanel_1.AddStackedItems(Button.button1, Button.button2, Button.button3));

            return Result.Succeeded;
        }
    }
}
