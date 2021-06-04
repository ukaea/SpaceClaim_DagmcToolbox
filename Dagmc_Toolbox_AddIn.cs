using System;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.IO;
using SpaceClaim.Api.V19.Extensibility;
using System.Windows.Forms;
using SpaceClaim.Api.V19.Geometry;

using Dagmc_Toolbox.Properties;


namespace Dagmc_Toolbox
{
    class Dagmc_Toolbox_AddIn : SpaceClaim.Api.V19.Extensibility.AddIn, SpaceClaim.Api.V19.Extensibility.IExtensibility, SpaceClaim.Api.V19.Extensibility.ICommandExtensibility, SpaceClaim.Api.V19.Extensibility.IRibbonExtensibility
    {

        readonly SpaceClaim.Api.V19.Extensibility.CommandCapsule[] capsules = new[]
        {
            new SpaceClaim.Api.V19.Extensibility.CommandCapsule("Dagmc_Toolbox.C#.V18.RibbonTab", Properties.Resources.RibbonTabText),
            new SpaceClaim.Api.V19.Extensibility.CommandCapsule("Dagmc_Toolbox.C#.V18.PartGroup", Properties.Resources.PartGroupText),
            new Commands.ExportDagmc(),
            //new Commands.PrintUid(),
            new Commands.CheckGeometry(),
            new Commands.CreateGroup()
            //new Commands.RemoteTrelis()
        };

        #region IExtensibility members
        public bool Connect()
        {
            // Initilization for add-in
            SpaceClaim.Api.V19.Unsupported.JournalMethods.RecordAutoLoadAddIn("SampleAddIn.C#.V18.RibbonTab", Properties.Resources.AddInManifestInfo);

            return true;
        }

        public void Disconnect()
        {

        }

        #endregion

        #region ICommandExtensibility members

        public void Initialize()
        {
            foreach (SpaceClaim.Api.V19.Extensibility.CommandCapsule capsule in capsules)
                capsule.Initialize();


            // Insert commands here for the context menu
        }

        #endregion

        #region IRibbonExtensibility members

        public string GetCustomUI()
        {
            return Properties.Resources.Ribbon;
        }

        #endregion
    }


}

