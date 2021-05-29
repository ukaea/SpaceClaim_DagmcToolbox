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
        protected override void OnExecute(Command command, ExecutionContext context, Rectangle buttonRect)
        {
            Debug.Assert(Window.ActiveWindow != null, "Window.ActiveWindow != null");
            /* 
             * Export objects to an .sat file depending upon their colour and material                          
            */

            // User selects the file destination to save
            //----------------------------------------------------------

            //MessageBox.Show("ExportDagmc called");
            /*
            var data = ExportDagmcData.FromString(context.Data);
            if (data == null)
            {
                data = PromptForData();
                context.Data = data.ToString(); // store the data to be used for journal replay
            }
            if (string.IsNullOrEmpty(data.FileName))
                return; // user canceled
            */

            // Declaration of SpaceClaim variables
            //----------------------------------------------------------

            Window window = Window.ActiveWindow;
            window.InteractionMode = InteractionMode.Solid;
            Document doc = window.Document;
            Part rootPart = doc.MainPart;
            ExportOptions options;
            options = null;
            string tempString = "_";
            //char[] delimiterChars = { '.' };  

            var exporter = new DagmcExporter();
            exporter.ExportedFileName = @"D:\test_moab_mesh.h5m";
            if (exporter.Execute())
            {
                MessageBox.Show("ExportDagmc to file " + exporter.ExportedFileName);
            }
        }



        // Prompt the user for the location to save the file
        //----------------------------------------------------------

        static ExportDagmcData PromptForData()
        {
            PartExportFormat exportFormat = PartExportFormat.AcisText;
            string fileName = null;

            AddIn.ExecuteWindowsFormsCode(() => {
                using (var fileDialog = new SaveFileDialog())
                {
                    var formats = new List<PartExportFormat>();

                    // add the supported file formats to the SaveAs dialog
                    string filter = string.Empty;
                    foreach (PartExportFormat format in Enum.GetValues(typeof(PartExportFormat)))
                    {
                        string formatFilter = GetFilter(format);
                        if (!string.IsNullOrEmpty(formatFilter))
                        {
                            filter += formatFilter + "|";
                            formats.Add(format);
                        }
                    }
                    filter = filter.TrimEnd('|');
                    fileDialog.Filter = filter;

                    if (fileDialog.ShowDialog(SpaceClaim.Api.V19.Application.MainWindow) != DialogResult.OK)
                        return; // user canceled

                    // get the data the user entered
                    exportFormat = formats[fileDialog.FilterIndex - 1];
                    fileName = fileDialog.FileName;
                }
            });

            return new ExportDagmcData(exportFormat, fileName);
        }


        // Different Filters for the user to choose from
        //----------------------------------------------------------

        static string GetFilter(PartExportFormat exportFormat)
        {
            switch (exportFormat)
            {
                case PartExportFormat.AcisText:
                    return "SAT files (*.sat)|*.sat";
                default:
                    return null;
            }
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
