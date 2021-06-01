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
    class CheckGeometry : CommandCapsule
    {
        // This command name must match that in the Ribbon.xml file
        //----------------------------------------------------------

        public const string CommandName = "Dagmc_Toolbox.C#.V18.CheckGeometry";
        //protected System.Collections.Concurrent.ConcurrentDictionary<IDesignBody, bool> CheckResults;

        public CheckGeometry() : base(CommandName, Resources.CheckGeometryText, Resources.CheckGeometry, Resources.CheckGeometryHint)
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
            var allBodies = Helper.GatherAllEntities<DesignBody>(rootPart);

            //var allVis = Helper.GatherAllVisibleBodies(rootPart, window);

            foreach (DesignBody body in allBodies)
            {
                var rawBody = body.Shape.Subject;  // null here
                System.Threading.ThreadPool.QueueUserWorkItem(new System.Threading.WaitCallback(this.CheckBody), rawBody);
                //SpaceClaim.Api.V19.Scripting.Internal.CustomMethods.QACheckBody(body, 70);
//#if API_LEVEL_19
                // tested it is fine in MainThread
                //var result = ApplicationHelper.CheckGeometry(Selection.Create(body));
//#endif
            }
        }

        protected void CheckBody(Object obj)
        {
            var rawBody = (SpaceClaim.Modeler.ISolidBody)obj;  // needs to reference to a few SpaceClaim assemblies
            List<SpaceClaim.CheckMessage> messages = new List<SpaceClaim.CheckMessage>();
            rawBody.Check(SpaceClaim.ModelerCheckLevel.CompleteCheck_70, true, messages);
            //
            //SpaceClaim.Api.V19.Scripting.Internal.CustomMethods.QACheckBody(designBody, 70);
        }
    }
}