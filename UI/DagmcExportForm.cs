using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Dagmc_Toolbox.UI
{
    public partial class DagmcExportForm : Form
    {
        public DagmcExportForm()
        {
            InitializeComponent();
        }

        internal double FacetTol
        {
            get { return (double)this.valueFacetTol.Value; }
            set { this.valueFacetTol.Value = (Decimal)value; }
        }

        internal int NormalTol
        {
            get { return (int)this.valueNormalTol.Value; }
            set { this.valueNormalTol.Value = value; }
        }

        internal double MaxAspectRatio
        {
            get { return (double)this.valueMaxAspectRatio.Value; }
            set { this.valueMaxAspectRatio.Value = (Decimal)value; }
        }
        internal double SurfaceDeviation
        {
            get { return (double)this.valueSurfaceDeviation.Value; }
            set { this.valueSurfaceDeviation.Value = (Decimal)value; }
        }
        /*        internal double LengthTol
                {
                    get { return (double)this.valueLengthTol.Value; }
                }*/

        internal bool Verbose
        {
            get { return this.checkBoxVerbose.Checked; }
            set { this.checkBoxVerbose.Checked = value; }
        }

        internal bool FatalOnCurve
        {
            get { return this.checkBoxFatalOnCurve.Checked; }
            set { this.checkBoxFatalOnCurve.Checked = value; }
        }

 
    }
}
