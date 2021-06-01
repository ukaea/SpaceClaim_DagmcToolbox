
namespace Dagmc_Toolbox.UI
{
    partial class DagmcExportForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
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
            this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
            this.valueNormalTol = new System.Windows.Forms.NumericUpDown();
            this.labelFacetTol = new System.Windows.Forms.Label();
            this.valueFacetTol = new System.Windows.Forms.NumericUpDown();
            this.checkBoxVerbose = new System.Windows.Forms.CheckBox();
            this.labelMaxAspectRatio = new System.Windows.Forms.Label();
            this.valueMaxAspectRatio = new System.Windows.Forms.NumericUpDown();
            this.label2 = new System.Windows.Forms.Label();
            this.checkBoxFatalOnCurve = new System.Windows.Forms.CheckBox();
            this.valueSurfaceDeviation = new System.Windows.Forms.NumericUpDown();
            this.labelVerbose = new System.Windows.Forms.Label();
            this.labelFatalOnCurve = new System.Windows.Forms.Label();
            this.labelNormalTol = new System.Windows.Forms.Label();
            this.cancelButton = new System.Windows.Forms.Button();
            this.okButton = new System.Windows.Forms.Button();
            this.tableLayoutPanel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.valueNormalTol)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueFacetTol)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueMaxAspectRatio)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueSurfaceDeviation)).BeginInit();
            this.SuspendLayout();
            // 
            // tableLayoutPanel1
            // 
            this.tableLayoutPanel1.ColumnCount = 2;
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 53.66795F));
            this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 46.33205F));
            this.tableLayoutPanel1.Controls.Add(this.valueNormalTol, 1, 1);
            this.tableLayoutPanel1.Controls.Add(this.labelFacetTol, 0, 0);
            this.tableLayoutPanel1.Controls.Add(this.valueFacetTol, 1, 0);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxVerbose, 1, 6);
            this.tableLayoutPanel1.Controls.Add(this.labelMaxAspectRatio, 0, 2);
            this.tableLayoutPanel1.Controls.Add(this.valueMaxAspectRatio, 1, 2);
            this.tableLayoutPanel1.Controls.Add(this.label2, 0, 3);
            this.tableLayoutPanel1.Controls.Add(this.checkBoxFatalOnCurve, 1, 4);
            this.tableLayoutPanel1.Controls.Add(this.valueSurfaceDeviation, 1, 3);
            this.tableLayoutPanel1.Controls.Add(this.labelVerbose, 0, 6);
            this.tableLayoutPanel1.Controls.Add(this.labelFatalOnCurve, 0, 4);
            this.tableLayoutPanel1.Controls.Add(this.labelNormalTol, 0, 1);
            this.tableLayoutPanel1.Location = new System.Drawing.Point(12, 24);
            this.tableLayoutPanel1.Name = "tableLayoutPanel1";
            this.tableLayoutPanel1.Padding = new System.Windows.Forms.Padding(3);
            this.tableLayoutPanel1.RowCount = 7;
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 50F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 47F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 38F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 51F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 28F));
            this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 85F));
            this.tableLayoutPanel1.Size = new System.Drawing.Size(518, 334);
            this.tableLayoutPanel1.TabIndex = 0;
            // 
            // valueNormalTol
            // 
            this.valueNormalTol.Location = new System.Drawing.Point(280, 45);
            this.valueNormalTol.Name = "valueNormalTol";
            this.valueNormalTol.Size = new System.Drawing.Size(120, 26);
            this.valueNormalTol.TabIndex = 5;
            // 
            // labelFacetTol
            // 
            this.labelFacetTol.AutoSize = true;
            this.labelFacetTol.Location = new System.Drawing.Point(6, 3);
            this.labelFacetTol.Name = "labelFacetTol";
            this.labelFacetTol.Size = new System.Drawing.Size(160, 20);
            this.labelFacetTol.TabIndex = 0;
            this.labelFacetTol.Text = "Facet tolerance (MM)";
            // 
            // valueFacetTol
            // 
            this.valueFacetTol.DecimalPlaces = 3;
            this.valueFacetTol.Increment = new decimal(new int[] {
            1,
            0,
            0,
            196608});
            this.valueFacetTol.Location = new System.Drawing.Point(280, 6);
            this.valueFacetTol.Name = "valueFacetTol";
            this.valueFacetTol.Size = new System.Drawing.Size(120, 26);
            this.valueFacetTol.TabIndex = 4;
            // 
            // checkBoxVerbose
            // 
            this.checkBoxVerbose.AutoSize = true;
            this.checkBoxVerbose.Location = new System.Drawing.Point(280, 248);
            this.checkBoxVerbose.Name = "checkBoxVerbose";
            this.checkBoxVerbose.Size = new System.Drawing.Size(22, 21);
            this.checkBoxVerbose.TabIndex = 6;
            this.checkBoxVerbose.UseVisualStyleBackColor = true;
            // 
            // labelMaxAspectRatio
            // 
            this.labelMaxAspectRatio.AutoSize = true;
            this.labelMaxAspectRatio.Location = new System.Drawing.Point(6, 81);
            this.labelMaxAspectRatio.Name = "labelMaxAspectRatio";
            this.labelMaxAspectRatio.Padding = new System.Windows.Forms.Padding(5, 5, 0, 0);
            this.labelMaxAspectRatio.Size = new System.Drawing.Size(139, 25);
            this.labelMaxAspectRatio.TabIndex = 8;
            this.labelMaxAspectRatio.Text = "Max Aspect Ratio";
            // 
            // valueMaxAspectRatio
            // 
            this.valueMaxAspectRatio.Location = new System.Drawing.Point(280, 84);
            this.valueMaxAspectRatio.Name = "valueMaxAspectRatio";
            this.valueMaxAspectRatio.Size = new System.Drawing.Size(120, 26);
            this.valueMaxAspectRatio.TabIndex = 9;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 128);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(131, 20);
            this.label2.TabIndex = 10;
            this.label2.Text = "SurfaceDeviation";
            // 
            // checkBoxFatalOnCurve
            // 
            this.checkBoxFatalOnCurve.AutoSize = true;
            this.checkBoxFatalOnCurve.Location = new System.Drawing.Point(280, 169);
            this.checkBoxFatalOnCurve.Name = "checkBoxFatalOnCurve";
            this.checkBoxFatalOnCurve.Size = new System.Drawing.Size(22, 21);
            this.checkBoxFatalOnCurve.TabIndex = 7;
            this.checkBoxFatalOnCurve.UseVisualStyleBackColor = true;
            // 
            // valueSurfaceDeviation
            // 
            this.valueSurfaceDeviation.Location = new System.Drawing.Point(280, 131);
            this.valueSurfaceDeviation.Name = "valueSurfaceDeviation";
            this.valueSurfaceDeviation.Size = new System.Drawing.Size(120, 26);
            this.valueSurfaceDeviation.TabIndex = 11;
            // 
            // labelVerbose
            // 
            this.labelVerbose.AutoSize = true;
            this.labelVerbose.Location = new System.Drawing.Point(6, 245);
            this.labelVerbose.Name = "labelVerbose";
            this.labelVerbose.Size = new System.Drawing.Size(69, 20);
            this.labelVerbose.TabIndex = 2;
            this.labelVerbose.Text = "Verbose";
            // 
            // labelFatalOnCurve
            // 
            this.labelFatalOnCurve.AutoSize = true;
            this.labelFatalOnCurve.Location = new System.Drawing.Point(6, 166);
            this.labelFatalOnCurve.Name = "labelFatalOnCurve";
            this.labelFatalOnCurve.Size = new System.Drawing.Size(247, 20);
            this.labelFatalOnCurve.TabIndex = 3;
            this.labelFatalOnCurve.Text = "Fatal Error On Curve mesh faillure";
            // 
            // labelNormalTol
            // 
            this.labelNormalTol.AutoSize = true;
            this.labelNormalTol.Location = new System.Drawing.Point(6, 42);
            this.labelNormalTol.Name = "labelNormalTol";
            this.labelNormalTol.Size = new System.Drawing.Size(182, 39);
            this.labelNormalTol.TabIndex = 1;
            this.labelNormalTol.Text = "Normal Tolerance/Angle Deviation(deg)";
            // 
            // cancelButton
            // 
            this.cancelButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.cancelButton.DialogResult = System.Windows.Forms.DialogResult.Abort;
            this.cancelButton.Location = new System.Drawing.Point(263, 379);
            this.cancelButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.cancelButton.Name = "cancelButton";
            this.cancelButton.Size = new System.Drawing.Size(112, 34);
            this.cancelButton.TabIndex = 27;
            this.cancelButton.Text = "&Cancel";
            // 
            // okButton
            // 
            this.okButton.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.okButton.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.okButton.Location = new System.Drawing.Point(384, 379);
            this.okButton.Margin = new System.Windows.Forms.Padding(4, 5, 4, 5);
            this.okButton.Name = "okButton";
            this.okButton.Size = new System.Drawing.Size(112, 34);
            this.okButton.TabIndex = 26;
            this.okButton.Text = "&OK";
            // 
            // DagmcExportForm
            // 
            this.AcceptButton = this.okButton;
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.cancelButton;
            this.ClientSize = new System.Drawing.Size(542, 450);
            this.Controls.Add(this.cancelButton);
            this.Controls.Add(this.okButton);
            this.Controls.Add(this.tableLayoutPanel1);
            this.Name = "DagmcExportForm";
            this.Text = "DagmcExportForm";
            this.tableLayoutPanel1.ResumeLayout(false);
            this.tableLayoutPanel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.valueNormalTol)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueFacetTol)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueMaxAspectRatio)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.valueSurfaceDeviation)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
        private System.Windows.Forms.Button cancelButton;
        private System.Windows.Forms.Button okButton;
        private System.Windows.Forms.Label labelFacetTol;
        private System.Windows.Forms.Label labelNormalTol;
        private System.Windows.Forms.Label labelVerbose;
        private System.Windows.Forms.Label labelFatalOnCurve;
        private System.Windows.Forms.NumericUpDown valueFacetTol;
        private System.Windows.Forms.NumericUpDown valueNormalTol;
        private System.Windows.Forms.CheckBox checkBoxFatalOnCurve;
        private System.Windows.Forms.CheckBox checkBoxVerbose;
        private System.Windows.Forms.Label labelMaxAspectRatio;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.NumericUpDown valueMaxAspectRatio;
        private System.Windows.Forms.NumericUpDown valueSurfaceDeviation;
    }
}