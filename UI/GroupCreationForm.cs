using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Diagnostics;

namespace Dagmc_Toolbox.UI
{
    /// <summary>
    ///
    ///  
    /// https://www.cfdsupport.com/OpenFOAM-Training-by-CFD-Support/node420.html
    /// </summary>
    partial class GroupCreationForm : Form
    {
        public GroupCreationForm(GroupAction action = GroupAction.Create, IEnumerable<string> groupNames = null)
        {
            InitializeComponent();
            this.Text = String.Format("About {0}", AssemblyTitle);
            this.GAction = action;

            if(null != groupNames)
                fillGroupNames(groupNames);
        }

        #region Assembly Attribute Accessors

        public string AssemblyTitle
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyTitleAttribute), false);
                if (attributes.Length > 0)
                {
                    AssemblyTitleAttribute titleAttribute = (AssemblyTitleAttribute)attributes[0];
                    if (titleAttribute.Title != "")
                    {
                        return titleAttribute.Title;
                    }
                }
                return System.IO.Path.GetFileNameWithoutExtension(Assembly.GetExecutingAssembly().CodeBase);
            }
        }

        public string AssemblyVersion
        {
            get
            {
                return Assembly.GetExecutingAssembly().GetName().Version.ToString();
            }
        }

        public string AssemblyDescription
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyDescriptionAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyDescriptionAttribute)attributes[0]).Description;
            }
        }

        public string AssemblyProduct
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyProductAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyProductAttribute)attributes[0]).Product;
            }
        }

        public string AssemblyCopyright
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCopyrightAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCopyrightAttribute)attributes[0]).Copyright;
            }
        }

        public string AssemblyCompany
        {
            get
            {
                object[] attributes = Assembly.GetExecutingAssembly().GetCustomAttributes(typeof(AssemblyCompanyAttribute), false);
                if (attributes.Length == 0)
                {
                    return "";
                }
                return ((AssemblyCompanyAttribute)attributes[0]).Company;
            }
        }

        #endregion

        private void groupAction_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;

            updateGroupActionUI();
        }

        void updateGroupActionUI()
        {
            if (radioButtonNew.Checked)
            {
                this.groupBoxNew.Visible = true;
                this.groupBoxExisting.Visible = false;
            }
            else
            {
                Debug.Assert(radioButtonAppend.Checked || radioButtonDelete.Checked);
                
                this.groupBoxNew.Visible = false;
                this.groupBoxExisting.Visible = true;
            }
        }

        /// <summary>
        /// return True is validation passed
        /// </summary>
        /// <returns></returns>
        internal bool ValidateForm()
        {
            if (this.GroupName == null || this.GroupName == string.Empty)
            {
                MessageBox.Show("GroupName is null or empty string, input or select from existent");
                return false;
            }
            if (this.GAction == GroupAction.Create || this.GAction == GroupAction.Append)
            {
                var objects = SpaceClaim.Api.V19.Window.ActiveWindow.ActiveContext.Selection;
                if (objects.Count == 0)
                {
                    MessageBox.Show("no shape objects selected, please select");
                }
            }
            MessageBox.Show("no error found, you can click OK button to perform action");
            return true;
        }

        internal GroupAction GAction
        {
            get 
            {
                if (radioButtonNew.Checked)
                {
                    return GroupAction.Create;
                }
                else if (radioButtonAppend.Checked)
                {
                    return GroupAction.Append;
                }
                else if (radioButtonDelete.Checked)
                {
                    return GroupAction.Delete;
                }
                else
                {
                    return GroupAction.Create;
                }
            }
            set 
            {
                if (GroupAction.Create == value)
                {
                    radioButtonNew.Checked = true; 
                }
                else if (GroupAction.Append == value)
                {
                    radioButtonAppend.Checked = true;
                }
                else if (GroupAction.Delete == value)
                {
                    radioButtonDelete.Checked = true;
                }
                updateGroupActionUI();
            }
        }
        internal GroupType GType
        {
            get
            {
                if (radioButtonMaterial.Checked)
                {
                    return GroupType.Material;
                }
                else if (radioButtonBoundary.Checked)
                {
                    return GroupType.Boundary;
                }
                else if (radioButtonImportance.Checked)
                {
                    return GroupType.Importance;
                }
                else
                {
                    return GroupType.Unknown;
                }
            }
            set
            {
                if (GroupType.Material == value)
                {
                    radioButtonMaterial.Checked = true;
                }
                else if (GroupType.Boundary == value)
                {
                    radioButtonBoundary.Checked = true;
                }
                else if (GroupType.Material == value)
                {
                    radioButtonDelete.Checked = true;
                }
            }
        }

        internal string GroupName
        {
            get 
            {
                if(GAction == GroupAction.Create)
                {
                    return textBoxGroupName.Text;
                }
                else
                {
                    return comboBoxGroupName.Text;
                }
            }
            //set 
            //{
            //    if (GroupAction != GroupActions.New)
            //    {
            //        // todo: set ComboBox
            //    }
            //}
        }

        void fillGroupNames(IEnumerable<string> names)
        {
            foreach (var name in names)
            {
                if(Helper.GetGroupType(name) != GroupType.Unknown)
                    comboBoxGroupName.Items.Add(name);
            }
        }

        private void comboBoxGroupName_SelectedIndexChanged(object sender, EventArgs e)
        {
            var o = (ComboBox)sender;
            GType = Helper.GetGroupType(o.Text);
        }

        private void buttonValidate_Click(object sender, EventArgs e)
        {
            this.ValidateForm();
        }
    }
}
