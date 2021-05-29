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


using Dagmc_Toolbox.Properties;


namespace Dagmc_Toolbox.Commands
{
    class CreateGroup : CommandCapsule
    {
        // This command name must match that in the Ribbon.xml file
        //----------------------------------------------------------

        public const string CommandName = "Dagmc_Toolbox.C#.V18.CreateGroup";

        public CreateGroup() : base(CommandName, Resources.CreateGroupText, Resources.CreateGroup, Resources.CreateGroupHint)
        {
        }

        protected override void OnUpdate(Command command)
        {
            command.IsEnabled = Window.ActiveWindow != null;
        }
        protected override void OnExecute(Command command, ExecutionContext context, Rectangle buttonRect)
        {
            Debug.Assert(Window.ActiveWindow != null, "Window.ActiveWindow != null");

            var groups = Helper.GatherAllGroups();   // 3 groups
            var names = groups.Select(C => C.Name);   // not
            using (var form = new UI.GroupCreationForm(GroupAction.Create, names))
            {
                if (form.ShowDialog() != DialogResult.OK)
                    return;
                
                ICollection<IDocObject> objects = null;
                if (form.GType == GroupType.Material) // here filter is assumed to be body only
                {
                    var selected = Helper.GatherSelectedObjects<DesignBody>(Window.ActiveWindow.ActiveContext);
                    objects = new List<IDocObject>();
                    foreach(var o in selected)
                        objects.Add((DocObject)o);
                }
                else if (form.GType == GroupType.Boundary)
                {
                    var selected = Helper.GatherSelectedObjects<DesignFace>(Window.ActiveWindow.ActiveContext);
                    objects = new List<IDocObject>();
                    foreach (var o in selected)
                        objects.Add((DocObject)o);
                }
                else
                {
                    objects = Window.ActiveWindow.ActiveContext.Selection;
                }

                /// parameter validations
                if(form.GroupName == null || form.GroupName == string.Empty)
                {
                    Debug.WriteLine("GroupName is null or empty string, just skip this group operation");
                }
                if (objects.Count == 0)
                    Debug.WriteLine("no shape objects selected, just skip this operation");

                if (form.GAction == GroupAction.Create)
                {
                    Group.Create(Helper.GetActiveMainPart(), form.GroupName, objects);
                }
                else if (form.GAction == GroupAction.Append)
                    Helper.AppendToGroup(objects, form.GroupName);
                else
                    return;  // DeleteGroup should be hidden from user for the time being.

            }
        }
    }
}
