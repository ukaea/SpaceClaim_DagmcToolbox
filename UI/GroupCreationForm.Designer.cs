
namespace Dagmc_Toolbox.UI
{
    partial class GroupCreationForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.groupBoxActions = new System.Windows.Forms.GroupBox();
            this.groupBoxExisting = new System.Windows.Forms.GroupBox();
            this.comboBoxGroupName = new System.Windows.Forms.ComboBox();
            this.radioButtonDelete = new System.Windows.Forms.RadioButton();
            this.radioButtonAppend = new System.Windows.Forms.RadioButton();
            this.groupBoxNew = new System.Windows.Forms.GroupBox();
            this.textBoxGroupName = new System.Windows.Forms.TextBox();
            this.radioButtonNew = new System.Windows.Forms.RadioButton();
            this.groupBoxTypes = new System.Windows.Forms.GroupBox();
            this.labelSubtype = new System.Windows.Forms.Label();
            this.comboSubtype = new System.Windows.Forms.ComboBox();
            this.labelScalarValue = new System.Windows.Forms.Label();
            this.numericUpDown1 = new System.Windows.Forms.NumericUpDown();
            this.radioButtonImportance = new System.Windows.Forms.RadioButton();
            this.radioButtonBoundary = new System.Windows.Forms.RadioButton();
            this.radioButtonMaterial = new System.Windows.Forms.RadioButton();
            this.okButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel = new System.Windows.Forms.TableLayoutPanel();
            this.groupBoxButtons = new System.Windows.Forms.GroupBox();
            this.cancelButton = new System.Windows.Forms.Button();
            this.richTextInfo = new System.Windows.Forms.RichTextBox();
            this.groupBoxActions.SuspendLayout();
            this.groupBoxExisting.SuspendLayout();
            this.groupBoxNew.SuspendLayout();
            this.groupBoxTypes.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).BeginInit();
            this.tableLayoutPanel.SuspendLayout();
            this.groupBoxButtons.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBoxActions
            // 
            this.groupBoxActions.Controls.Add(this.groupBoxExisting);
            this.groupBoxActions.Controls.Add(this.radioButtonDelete);
            this.groupBoxActions.Controls.Add(this.radioButtonAppend);
            this.groupBoxActions.Controls.Add(this.groupBoxNew);
            this.groupBoxActions.Controls.Add(this.radioButtonNew);
            this.groupBoxActions.Location = new System.Drawing.Point(21, 11);
            this.groupBoxActions.Name = "groupBoxActions";
            this.groupBoxActions.Size = new System.Drawing.Size(489, 179);
            this.groupBoxActions.TabIndex = 28;
            this.groupBoxActions.TabStop = false;
            this.groupBoxActions.Text = "Actions";
            // 
            // groupBoxExisting
            // 
            this.groupBoxExisting.Controls.Add(this.comboBoxGroupName);
            this.groupBoxExisting.Location = new System.Drawing.Point(0, 115);
            this.groupBoxExisting.Name = "groupBoxExisting";
            this.groupBoxExisting.Size = new System.Drawing.Size(430, 53);
            this.groupBoxExisting.TabIndex = 30;
            this.groupBoxExisting.TabStop = false;
            this.groupBoxExisting.Text = "Select an existing group";
            // 
            // comboBoxGroupName
            // 
            this.comboBoxGroupName.FormattingEnabled = true;
            this.comboBoxGroupName.Location = new System.Drawing.Point(12, 21);
            this.comboBoxGroupName.Name = "comboBoxGroupName";
            this.comboBoxGroupName.Size = new System.Drawing.Size(174, 24);
            this.comboBoxGroupName.TabIndex = 28;
            this.comboBoxGroupName.SelectedIndexChanged += new System.EventHandler(this.comboBoxGroupName_SelectedIndexChanged);
            // 
            // radioButtonDelete
            // 
            this.radioButtonDelete.AutoSize = true;
            this.radioButtonDelete.Enabled = false;
            this.radioButtonDelete.Location = new System.Drawing.Point(356, 22);
            this.radioButtonDelete.Name = "radioButtonDelete";
            this.radioButtonDelete.Size = new System.Drawing.Size(109, 21);
            this.radioButtonDelete.TabIndex = 27;
            this.radioButtonDelete.TabStop = true;
            this.radioButtonDelete.Text = "delete group";
            this.radioButtonDelete.UseVisualStyleBackColor = true;
            // 
            // radioButtonAppend
            // 
            this.radioButtonAppend.AutoSize = true;
            this.radioButtonAppend.Location = new System.Drawing.Point(178, 21);
            this.radioButtonAppend.Name = "radioButtonAppend";
            this.radioButtonAppend.Size = new System.Drawing.Size(170, 21);
            this.radioButtonAppend.TabIndex = 26;
            this.radioButtonAppend.TabStop = true;
            this.radioButtonAppend.Text = "add selection to group";
            this.radioButtonAppend.UseVisualStyleBackColor = true;
            // 
            // groupBoxNew
            // 
            this.groupBoxNew.Controls.Add(this.textBoxGroupName);
            this.groupBoxNew.Location = new System.Drawing.Point(0, 49);
            this.groupBoxNew.Name = "groupBoxNew";
            this.groupBoxNew.Size = new System.Drawing.Size(430, 60);
            this.groupBoxNew.TabIndex = 29;
            this.groupBoxNew.TabStop = false;
            this.groupBoxNew.Text = "Give the new group a name";
            // 
            // textBoxGroupName
            // 
            this.textBoxGroupName.Location = new System.Drawing.Point(12, 25);
            this.textBoxGroupName.Name = "textBoxGroupName";
            this.textBoxGroupName.Size = new System.Drawing.Size(174, 22);
            this.textBoxGroupName.TabIndex = 0;
            this.textBoxGroupName.Text = "a new group name";
            // 
            // radioButtonNew
            // 
            this.radioButtonNew.AutoSize = true;
            this.radioButtonNew.Location = new System.Drawing.Point(6, 21);
            this.radioButtonNew.Name = "radioButtonNew";
            this.radioButtonNew.Size = new System.Drawing.Size(153, 21);
            this.radioButtonNew.TabIndex = 25;
            this.radioButtonNew.Text = "Create a new group";
            this.radioButtonNew.UseVisualStyleBackColor = true;
            this.radioButtonNew.CheckedChanged += new System.EventHandler(this.groupAction_CheckedChanged);
            // 
            // groupBoxTypes
            // 
            this.groupBoxTypes.Controls.Add(this.labelSubtype);
            this.groupBoxTypes.Controls.Add(this.comboSubtype);
            this.groupBoxTypes.Controls.Add(this.labelScalarValue);
            this.groupBoxTypes.Controls.Add(this.numericUpDown1);
            this.groupBoxTypes.Controls.Add(this.radioButtonImportance);
            this.groupBoxTypes.Controls.Add(this.radioButtonBoundary);
            this.groupBoxTypes.Controls.Add(this.radioButtonMaterial);
            this.groupBoxTypes.Location = new System.Drawing.Point(3, 193);
            this.groupBoxTypes.Name = "groupBoxTypes";
            this.groupBoxTypes.Size = new System.Drawing.Size(495, 92);
            this.groupBoxTypes.TabIndex = 26;
            this.groupBoxTypes.TabStop = false;
            this.groupBoxTypes.Text = "Group Types (todo: make a user control)";
            // 
            // labelSubtype
            // 
            this.labelSubtype.AutoSize = true;
            this.labelSubtype.Location = new System.Drawing.Point(9, 54);
            this.labelSubtype.Name = "labelSubtype";
            this.labelSubtype.Size = new System.Drawing.Size(80, 17);
            this.labelSubtype.TabIndex = 34;
            this.labelSubtype.Text = "Material/BC";
            // 
            // comboSubtype
            // 
            this.comboSubtype.FormattingEnabled = true;
            this.comboSubtype.Location = new System.Drawing.Point(106, 54);
            this.comboSubtype.Name = "comboSubtype";
            this.comboSubtype.Size = new System.Drawing.Size(174, 24);
            this.comboSubtype.TabIndex = 33;
            // 
            // labelScalarValue
            // 
            this.labelScalarValue.AutoSize = true;
            this.labelScalarValue.Location = new System.Drawing.Point(300, 54);
            this.labelScalarValue.Name = "labelScalarValue";
            this.labelScalarValue.Size = new System.Drawing.Size(44, 17);
            this.labelScalarValue.TabIndex = 29;
            this.labelScalarValue.Text = "Value";
            // 
            // numericUpDown1
            // 
            this.numericUpDown1.Location = new System.Drawing.Point(362, 56);
            this.numericUpDown1.Name = "numericUpDown1";
            this.numericUpDown1.Size = new System.Drawing.Size(120, 22);
            this.numericUpDown1.TabIndex = 28;
            // 
            // radioButtonImportance
            // 
            this.radioButtonImportance.AutoSize = true;
            this.radioButtonImportance.Location = new System.Drawing.Point(362, 21);
            this.radioButtonImportance.Name = "radioButtonImportance";
            this.radioButtonImportance.Size = new System.Drawing.Size(99, 21);
            this.radioButtonImportance.TabIndex = 27;
            this.radioButtonImportance.TabStop = true;
            this.radioButtonImportance.Text = "importance";
            this.radioButtonImportance.UseVisualStyleBackColor = true;
            // 
            // radioButtonBoundary
            // 
            this.radioButtonBoundary.AutoSize = true;
            this.radioButtonBoundary.Location = new System.Drawing.Point(184, 21);
            this.radioButtonBoundary.Name = "radioButtonBoundary";
            this.radioButtonBoundary.Size = new System.Drawing.Size(150, 21);
            this.radioButtonBoundary.TabIndex = 26;
            this.radioButtonBoundary.TabStop = true;
            this.radioButtonBoundary.Text = "boundary condition";
            this.radioButtonBoundary.UseVisualStyleBackColor = true;
            // 
            // radioButtonMaterial
            // 
            this.radioButtonMaterial.AutoSize = true;
            this.radioButtonMaterial.Location = new System.Drawing.Point(12, 21);
            this.radioButtonMaterial.Name = "radioButtonMaterial";
            this.radioButtonMaterial.Size = new System.Drawing.Size(79, 21);
            this.radioButtonMaterial.TabIndex = 25;
            this.radioButtonMaterial.Text = "material";
            this.radioButtonMaterial.UseVisualStyleBackColor = true;
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(361, 13);
            this.okButton.Margin = new System.Windows.Forms.Padding(4);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(100, 27);
            this.okButton.TabIndex = 24;
            this.okButton.Text = "&OK";
            // 
            // tableLayoutPanel
            // 
            this.tableLayoutPanel.AutoSize = true;
            this.tableLayoutPanel.ColumnCount = 1;
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
            this.tableLayoutPanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel.Controls.Add(this.groupBoxTypes, 0, 1);
            this.tableLayoutPanel.Controls.Add(this.groupBoxButtons, 0, 3);
            this.tableLayoutPanel.Controls.Add(this.richTextInfo, 0, 2);
            this.tableLayoutPanel.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tableLayoutPanel.Location = new System.Drawing.Point(12, 11);
            this.tableLayoutPanel.Margin = new System.Windows.Forms.Padding(4);
            this.tableLayoutPanel.Name = "tableLayoutPanel";
            this.tableLayoutPanel.RowCount = 4;
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 46.45477F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 24.93888F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 28.60636F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 52F));
            this.tableLayoutPanel.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 20F));
            this.tableLayoutPanel.Size = new System.Drawing.Size(501, 462);
            this.tableLayoutPanel.TabIndex = 0;
            // 
            // groupBoxButtons
            // 
            this.groupBoxButtons.Controls.Add(this.cancelButton);
            this.groupBoxButtons.Controls.Add(this.okButton);
            this.groupBoxButtons.Location = new System.Drawing.Point(3, 412);
            this.groupBoxButtons.Name = "groupBoxButtons";
            this.groupBoxButtons.Size = new System.Drawing.Size(495, 47);
            this.groupBoxButtons.TabIndex = 32;
            this.groupBoxButtons.TabStop = false;
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.cancelButton.Location = new System.Drawing.Point(253, 13);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(100, 27);
            this.cancelButton.TabIndex = 25;
            this.cancelButton.Text = "&Cancel";
            // 
            // richTextInfo
            // 
            this.richTextInfo.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.richTextInfo.Location = new System.Drawing.Point(3, 295);
            this.richTextInfo.Name = "richTextInfo";
            this.richTextInfo.Size = new System.Drawing.Size(495, 111);
            this.richTextInfo.TabIndex = 33;
            this.richTextInfo.Text = "";
            // 
            // GroupCreationForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(8F, 16F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(525, 484);
            this.Controls.Add(this.groupBoxActions);
            this.Controls.Add(this.tableLayoutPanel);
            this.Margin = new System.Windows.Forms.Padding(4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "GroupCreationForm";
            this.Padding = new System.Windows.Forms.Padding(12, 11, 12, 11);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "Create Group";
            this.groupBoxActions.ResumeLayout(false);
            this.groupBoxActions.PerformLayout();
            this.groupBoxExisting.ResumeLayout(false);
            this.groupBoxNew.ResumeLayout(false);
            this.groupBoxNew.PerformLayout();
            this.groupBoxTypes.ResumeLayout(false);
            this.groupBoxTypes.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.numericUpDown1)).EndInit();
            this.tableLayoutPanel.ResumeLayout(false);
            this.groupBoxButtons.ResumeLayout(false);
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion
        private System.Windows.Forms.GroupBox groupBoxActions;
        private System.Windows.Forms.RadioButton radioButtonAppend;
        private System.Windows.Forms.RadioButton radioButtonNew;
        private System.Windows.Forms.GroupBox groupBoxTypes;
        private System.Windows.Forms.RadioButton radioButtonImportance;
        private System.Windows.Forms.RadioButton radioButtonBoundary;
        private System.Windows.Forms.RadioButton radioButtonMaterial;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel;
        private System.Windows.Forms.GroupBox groupBoxExisting;
        private System.Windows.Forms.ComboBox comboBoxGroupName;
        private System.Windows.Forms.GroupBox groupBoxNew;
        private System.Windows.Forms.TextBox textBoxGroupName;
        private System.Windows.Forms.GroupBox groupBoxButtons;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.RadioButton radioButtonDelete;
        private System.Windows.Forms.Label labelScalarValue;
        private System.Windows.Forms.NumericUpDown numericUpDown1;
        private System.Windows.Forms.Label labelSubtype;
        private System.Windows.Forms.ComboBox comboSubtype;
        private System.Windows.Forms.RichTextBox richTextInfo;
    }
}
