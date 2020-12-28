using CR2WLib;
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

            CR2WVariant resources = file.Exports[0].Data["resources"];
            CR2WVariant descriptions = null;
            
            if (file.Exports[0].Data.ContainsKey("descriptions"))
                descriptions = file.Exports[0].Data["descriptions"];

            if (resources.TypeName != "array:raRef:CResource")
                return;

            if (descriptions != null && descriptions.TypeName != "array:String")
                return;

            string[] resourceTable = resources.ToRaRefArray();
            string[] descriptionTable = null;

            if (descriptions != null)
                descriptionTable = descriptions.ToStringArray();
            
            for (int i=0; i < resourceTable.Length; i++)
            {
                DataGridViewRow row = new DataGridViewRow();
                row.CreateCells(this.resourceListView);

                row.Cells[0].Value = resourceTable[i];

                if (descriptionTable != null)
                    row.Cells[1].Value = descriptionTable[i];

                this.resourceListView.Rows.Add(row);
            }
        }
    }
}
