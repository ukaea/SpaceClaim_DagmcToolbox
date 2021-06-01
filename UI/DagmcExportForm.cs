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
        }

        internal int NormalTol
        {
            get { return (int)this.valueNormalTol.Value; }
        }

/*        internal double LengthTol
        {
            get { return (double)this.valueLengthTol.Value; }
        }*/

        internal bool Verbose
        {
            get { return this.checkBoxVerbose.Checked; }
        }

        internal bool FatalOnCurve
        {
            get { return this.checkBoxFatalOnCurve.Checked; }
        }
    }
}
