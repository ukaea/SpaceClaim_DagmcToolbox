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

            var groups = Helper.GatherAllGroups();   // could be empty
            List<string> names = new List<string>();
            foreach(var g in groups)
            {
                names.Add(g.Name);
            }
            using (var form = new UI.GroupCreationForm(GroupAction.Create, names))
            {
                // ShowDialog()  will show form as Modal Dialog, other windows is not operable
                // user must select 
                if (form.ShowDialog() != DialogResult.OK)
                    return;
                
                ICollection<IDocObject> objects = null;
                if (form.GType == GroupType.Material) // here filter is assumed to be body only
                {
                    objects = new List<IDocObject>();
                    var selected = Window.ActiveWindow.ActiveContext.Selection;
                    foreach (var o in selected)
                    {
                        if(o is IDesignBody)
                            objects.Add(o as IDesignBody);
                    }
                }
                else if (form.GType == GroupType.Boundary) // here filter is assumed to be face only
                {
                    objects = new List<IDocObject>();
                    var selected = Window.ActiveWindow.ActiveContext.Selection;
                    foreach (var o in selected)
                    {
                        if (o is IDesignFace)
                            objects.Add(o as IDesignFace);
                        else
                            Debug.WriteLine("Warning: Boundary is assumed to be surface only, no edge is added");
                    }
                    
                }
                else  // form.GType == GroupType.Boundary, add all selected DocObject
                {
                    objects = Window.ActiveWindow.ActiveContext.Selection;
                }

                // todo: make the full group name, that can be saved to MOAB
                if (form.GAction == GroupAction.Create)
                {
                    if (objects.Count == 0)
                        return;
                    Group.Create(Helper.GetActiveMainPart(), form.GroupName, objects);
                }
                else if (form.GAction == GroupAction.Append)
                {
                    if (objects.Count == 0)
                        return;
                    Helper.AppendToGroup(objects, form.GroupName);
                }
                else
                    return;  // DeleteGroup has been hidden/disabled from user for the time being.
            }
        }
    }
}
