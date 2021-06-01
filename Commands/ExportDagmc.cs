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
    class ExportDagmc : CommandCapsule
    {
        // This command name must match that in the Ribbon.xml file
        //----------------------------------------------------------

        public const string CommandName = "Dagmc_Toolbox.C#.V18.ExportDagmc";

        public ExportDagmc() : base(CommandName, Resources.ExportDagmcText, Resources.ExportDagmc, Resources.ExportDagmcHint)
        {
        }
        
        protected override void OnUpdate(Command command)
        {
            command.IsEnabled = Window.ActiveWindow != null;
        }
        /// <summary>
        /// export mesh for DAGMC workflow
        /// </summary>
        /// <param name="command"></param>
        /// <param name="context"></param>
        /// <param name="buttonRect"></param>
        protected override void OnExecute(Command command, ExecutionContext context, Rectangle buttonRect)
        {
            Debug.Assert(Window.ActiveWindow != null, "Window.ActiveWindow != null");

            //MessageBox.Show("ExportDagmc called");

            var exportor = new DagmcExporter();
            //
            exportor.ExportedFileName = GetExportFilename();
            if (exportor.Execute())
            {
                MessageBox.Show("ExportDagmc to file " + exportor.ExportedFileName);
            }
        }


        /// <summary>
        /// Prompt the user for the location to save the file
        /// </summary>
        /// <returns> file path </returns>
        static string GetExportFilename()
        {
            string filename = @"D:\test_moab_mesh.h5m"; 
            AddIn.ExecuteWindowsFormsCode(() =>
            {
                using (var fileDialog = new SaveFileDialog())
                {
                    fileDialog.Filter = "HDF5Mesh(*.h5m)|*.h5m;|VTKMesh(*.vtk)|*.vtk";
                    fileDialog.FileName = "Dagmc.h5m";
                    fileDialog.DefaultExt = "h5m";
                    if (fileDialog.ShowDialog() == DialogResult.OK)
                    {
                        filename = fileDialog.FileName;
                    }
                }
            });
            return filename;
        }

    }


    // Class of export properties 
    //----------------------------------------------------------

    public class ExportDagmcData : CommandData<ExportDagmcData>
    {
        public ExportDagmcData(PartExportFormat format, string filename)
        {
            Format = format;
            FileName = filename;
        }

        protected ExportDagmcData()
        {
            // Serialization
        }

        [XmlElement(ElementName = "File")]
        public string FileName { get; set; }

        [XmlElement(ElementName = "Format")]
        public PartExportFormat Format { get; set; }
    }

}
