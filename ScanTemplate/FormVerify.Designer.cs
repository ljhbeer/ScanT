﻿namespace ScanTemplate
{
    partial class FormVerify
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
            this.listBoxXH = new System.Windows.Forms.ListBox();
            this.label1 = new System.Windows.Forms.Label();
            this.listBoxXZT = new System.Windows.Forms.ListBox();
            this.label2 = new System.Windows.Forms.Label();
            this.buttonImportData = new System.Windows.Forms.Button();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.SuspendLayout();
            // 
            // listBoxXH
            // 
            this.listBoxXH.FormattingEnabled = true;
            this.listBoxXH.ItemHeight = 12;
            this.listBoxXH.Location = new System.Drawing.Point(12, 17);
            this.listBoxXH.Name = "listBoxXH";
            this.listBoxXH.Size = new System.Drawing.Size(109, 376);
            this.listBoxXH.TabIndex = 0;
            this.listBoxXH.SelectedIndexChanged += new System.EventHandler(this.listBoxXH_SelectedIndexChanged);
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(27, 2);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(53, 12);
            this.label1.TabIndex = 1;
            this.label1.Text = "学号校验";
            // 
            // listBoxXZT
            // 
            this.listBoxXZT.FormattingEnabled = true;
            this.listBoxXZT.ItemHeight = 12;
            this.listBoxXZT.Location = new System.Drawing.Point(127, 17);
            this.listBoxXZT.Name = "listBoxXZT";
            this.listBoxXZT.Size = new System.Drawing.Size(109, 376);
            this.listBoxXZT.TabIndex = 0;
            this.listBoxXZT.SelectedIndexChanged += new System.EventHandler(this.listBoxXZT_SelectedIndexChanged);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(143, 2);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(65, 12);
            this.label2.TabIndex = 1;
            this.label2.Text = "选择题校验";
            // 
            // buttonImportData
            // 
            this.buttonImportData.Location = new System.Drawing.Point(20, 411);
            this.buttonImportData.Name = "buttonImportData";
            this.buttonImportData.Size = new System.Drawing.Size(83, 34);
            this.buttonImportData.TabIndex = 2;
            this.buttonImportData.Text = "导入数据";
            this.buttonImportData.UseVisualStyleBackColor = true;
            this.buttonImportData.Click += new System.EventHandler(this.buttonImportData_Click);
            // 
            // pictureBox1
            // 
            this.pictureBox1.Location = new System.Drawing.Point(252, 18);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(335, 375);
            this.pictureBox1.TabIndex = 3;
            this.pictureBox1.TabStop = false;
            // 
            // FormVerify
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(599, 443);
            this.Controls.Add(this.pictureBox1);
            this.Controls.Add(this.buttonImportData);
            this.Controls.Add(this.label2);
            this.Controls.Add(this.label1);
            this.Controls.Add(this.listBoxXZT);
            this.Controls.Add(this.listBoxXH);
            this.Name = "FormVerify";
            this.Text = "FormVerify";
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.ListBox listBoxXH;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.ListBox listBoxXZT;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button buttonImportData;
        private System.Windows.Forms.PictureBox pictureBox1;
    }
}