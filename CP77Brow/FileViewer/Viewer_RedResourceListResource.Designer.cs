
namespace CP77Brow.FileViewer
{
    partial class Viewer_RedResourceListResource
    {
        /// <summary> 
        /// Erforderliche Designervariable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Verwendete Ressourcen bereinigen.
        /// </summary>
        /// <param name="disposing">True, wenn verwaltete Ressourcen gelöscht werden sollen; andernfalls False.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Vom Komponenten-Designer generierter Code

        /// <summary> 
        /// Erforderliche Methode für die Designerunterstützung. 
        /// Der Inhalt der Methode darf nicht mit dem Code-Editor geändert werden.
        /// </summary>
        private void InitializeComponent()
        {
            this.resourceListView = new System.Windows.Forms.DataGridView();
            this.Resource = new System.Windows.Forms.DataGridViewTextBoxColumn();
            this.Description = new System.Windows.Forms.DataGridViewTextBoxColumn();
            ((System.ComponentModel.ISupportInitialize)(this.resourceListView)).BeginInit();
            this.SuspendLayout();
            // 
            // resourceListView
            // 
            this.resourceListView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            this.resourceListView.Columns.AddRange(new System.Windows.Forms.DataGridViewColumn[] {
            this.Resource,
            this.Description});
            this.resourceListView.Dock = System.Windows.Forms.DockStyle.Fill;
            this.resourceListView.Location = new System.Drawing.Point(0, 0);
            this.resourceListView.Name = "resourceListView";
            this.resourceListView.Size = new System.Drawing.Size(653, 503);
            this.resourceListView.TabIndex = 0;
            // 
            // Resource
            // 
            this.Resource.AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.Fill;
            this.Resource.HeaderText = "Resource";
            this.Resource.Name = "Resource";
            // 
            // Description
            // 
            this.Description.HeaderText = "Description";
            this.Description.Name = "Description";
            // 
            // Viewer_RedResourceListResource
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.resourceListView);
            this.Name = "Viewer_RedResourceListResource";
            this.Size = new System.Drawing.Size(653, 503);
            ((System.ComponentModel.ISupportInitialize)(this.resourceListView)).EndInit();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.DataGridView resourceListView;
        private System.Windows.Forms.DataGridViewTextBoxColumn Resource;
        private System.Windows.Forms.DataGridViewTextBoxColumn Description;
    }
}
