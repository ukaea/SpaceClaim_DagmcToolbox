using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using SpaceClaim.Api.V19.Extensibility;
using SpaceClaim.Api.V19.Geometry;
using SpaceClaim.Api.V19.Modeler;
using System.Xml.Serialization;
using System.Windows.Forms;
using SpaceClaim.Api.V19;
using Point = SpaceClaim.Api.V19.Geometry.Point;
using System.Diagnostics;

using SpaceClaim.Api.V19.Scripting.Helpers;
using SpaceClaim.Api.V19.Scripting.Selection;

using Dagmc_Toolbox.Properties;


namespace Dagmc_Toolbox.Commands
{
    class RemoteTrelis : CommandCapsule
    {
        // This command name must match that in the Ribbon.xml file
        //----------------------------------------------------------

        public const string CommandName = "Dagmc_Toolbox.C#.V18.RemoteTrelis";
        protected System.Collections.Concurrent.ConcurrentDictionary<IDesignBody, bool> CheckResults;

        public RemoteTrelis() : base(CommandName, Resources.RemoteTrelisText, Resources.RemoteTrelis, Resources.RemoteTrelisHint)
        {
        }

        protected override void OnUpdate(Command command)
        {
            command.IsEnabled = Window.ActiveWindow != null;
        }
        protected override void OnExecute(Command command, ExecutionContext context, Rectangle buttonRect)
        {
            Debug.Assert(Window.ActiveWindow != null, "Window.ActiveWindow != null");

            Window window = Window.ActiveWindow;
            window.InteractionMode = InteractionMode.Solid;
            Document doc = window.Document;
            Part rootPart = doc.MainPart;

            string workdir = "d:\\";
            GenerateManifest(rootPart, workdir);
            RunRemoteTrelis(workdir);
        }

        protected void GenerateManifest(IPart part, string workdir)
        {
            var allBodies = Helper.GatherAllEntities<DesignBody>(part);
            //var allVis = Helper.GatherAllVisibleBodies(rootPart, window);
            var lines = new List<string> { "[" };
            foreach (DesignBody body in allBodies)
            {
                //export body into a file
                // Path.Combine(workdir, filename)
                string bodyname = body.Name;  // todo not unique
                string filename = bodyname + ".sat";
                body.Save(BodySaveFormat.Text, filename);  // save to sat only
                string matname = "Unknown";
                if (body.Material != null)  // todo: get materail info from group
                {
                    matname = body.Material.Name;
                }
                string[] entryLines = { 
                    "{", 
                        $"    \"filename\": \"{filename}\",",
                        $"    \"material\": \"{matname}\"",
                    "}" };
                lines.AddRange(entryLines);
            }
            lines.Append("]");
            File.WriteAllLines(Path.Combine(workdir, "manifest.json"), lines);
        }

        protected void RunRemoteTrelis(string workdir)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo();
            startInfo.FileName = "cmd.exe";
            bool redirecting = true;
            startInfo.UseShellExecute = !redirecting;   // start a new windows
            //startInfo.WindowStyle = ProcessWindowStyle.Minimized;  // do not hide the windows
            startInfo.WorkingDirectory = workdir;  // set this can avoid path string escape
            startInfo.Arguments = @" /c python D:\MyData\StepMultiphysics\integrated_simulation\remote_solver\remotetrelis\__main__.py";
            startInfo.RedirectStandardError = redirecting;
            startInfo.RedirectStandardOutput = redirecting;

            var _proc = Process.Start(startInfo);
            // do not wait for completion, manually close the windows
            _proc.WaitForExit();

            // get output as log file
            if (redirecting)
            {
                var error = _proc.StandardError.ReadToEnd();
                //Console.WriteLine("Error: " + error);
                var output = _proc.StandardOutput.ReadToEnd();
                //Console.WriteLine("Output: " + output);
                string[] output_lines = { error, output };
                File.WriteAllLines(Path.Combine(workdir, "remote_trelis.log"), output_lines);
            }

        }
    }
}