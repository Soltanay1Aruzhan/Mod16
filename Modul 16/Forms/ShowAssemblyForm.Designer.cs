namespace FManagerApp.Forms
{
    partial class ShowAssemblyForm
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
            this.AssemblyTreeView = new System.Windows.Forms.TreeView();
            this.SuspendLayout();
            // 
            // AssemblyTreeView
            // 
            this.AssemblyTreeView.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.AssemblyTreeView.Location = new System.Drawing.Point(13, 13);
            this.AssemblyTreeView.Name = "AssemblyTreeView";
            this.AssemblyTreeView.Size = new System.Drawing.Size(601, 467);
            this.AssemblyTreeView.TabIndex = 0;
            // 
            // ShowAssemblyForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(626, 492);
            this.Controls.Add(this.AssemblyTreeView);
            this.Name = "ShowAssemblyForm";
            this.Load += new System.EventHandler(this.ShowAssemblyForm_Load);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.TreeView AssemblyTreeView;
    }
}