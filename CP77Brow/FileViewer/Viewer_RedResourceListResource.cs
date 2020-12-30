using CR2WLib;
using CR2WLib.Types;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace CP77Brow.FileViewer
{
    public partial class Viewer_RedResourceListResource : UserControl
    {
        public Viewer_RedResourceListResource(CR2WFile file)
        {
            InitializeComponent();

            CR2WValue[] resources = file.Exports[0].NewData["resources"].As<CR2WValue[]>();
            CR2WValue[] descriptions = null;
            
            if (file.Exports[0].NewData.ContainsKey("descriptions"))
                descriptions = file.Exports[0].NewData["descriptions"].As<CR2WValue[]>();
            
            for (int i=0; i < resources.Length; i++)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(this.resourceListView);

                row.Cells[0].Value = resources[i].As<CR2WImport>().Path;

                if (descriptions != null)
                    row.Cells[1].Value = descriptions[i].As<string>();

                this.resourceListView.Rows.Add(row);
            }
        }
    }
}
