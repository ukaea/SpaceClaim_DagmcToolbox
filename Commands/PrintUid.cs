using System;
using System.IO;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using SpaceClaim.Api.V19.Extensibility;
using SpaceClaim.Api.V19.Geometry;
using SpaceClaim.Api.V19.Modeler;
using SpaceClaim.Api.V19;
using SpaceClaim.Api.V19.Display;
using Point = SpaceClaim.Api.V19.Geometry.Point;
using SpaceClaim.Api.V19.Scripting;
using Dagmc_Toolbox.Properties;

namespace Dagmc_Toolbox.Commands
{
    //class ScriptClass : 

    class PrintUid : CommandCapsule
    {
        // This command name must match that in the Ribbon.xml file
        //----------------------------------------------------------
        public string ScriptRelPath = @"PythonScripts\PrintUid.py";
        public const string CommandName = "Dagmc_Toolbox.C#.V18.PrintUid";

        public PrintUid() : base(CommandName, Resources.PrintUidText, Resources.PrintUid, Resources.PrintUidHint)
        {

        }

        protected override void OnExecute(Command command, ExecutionContext context, Rectangle buttonRect)
        {
            var l = Assembly.GetAssembly(typeof(PrintUid)).Location;
            string assemblyDir = Path.GetDirectoryName(l);
            var scriptPath = Path.Combine(assemblyDir, ScriptRelPath);
            if (File.Exists(scriptPath))
            {
                // Variables
                Window window = Window.ActiveWindow;
                Document doc = window.Document;
                Part rootPart = doc.MainPart;

                // To pass args to python script
                var scriptParams = new Dictionary<string, object>();
                //scriptParams.Add("iter", iterations);
                //scriptParams.Add("mf", maxfaces);

                // Run the script
                SpaceClaim.Api.V19.Application.RunScript(scriptPath, scriptParams);
                //MessageBox.Show($"INFO: Script file {scriptPath} called successfully");
            }
            else
            {
                MessageBox.Show($"ERROR: Script file {scriptPath} is not found.");
            }
        }
    }
}