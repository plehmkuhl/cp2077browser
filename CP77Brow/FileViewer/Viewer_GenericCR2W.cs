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
    public partial class Viewer_GenericCR2W : UserControl
    {
        public Viewer_GenericCR2W(CR2WFile file)
        {
            InitializeComponent();

            // Names
            {
                TreeNode namesNode = new TreeNode("ClassNames");
                foreach (string name in file.CNames)
                {
                    namesNode.Nodes.Add(name);
                }

                this.treeView.Nodes.Add(namesNode);
            }

            // Imports
            {
                TreeNode importsNode = new TreeNode("Imports");
                foreach (CR2WImport import in file.Imports)
                {
                    importsNode.Nodes.Add($"{import.Path} | {import.ClassName} |{import.Flags}");
                }

                this.treeView.Nodes.Add(importsNode);
            }

            // Properties
            {
                TreeNode propertiesNode = new TreeNode("Properties");
                foreach (CR2WProperty property in file.Properties)
                {
                    propertiesNode.Nodes.Add($"{property.Name} => {property.ClassName}");
                }

                this.treeView.Nodes.Add(propertiesNode);
            }

            // Exports
            {
                TreeNode exportsNode = new TreeNode("Exports");
                foreach (CR2WExport export in file.Exports)
                {
                    TreeNode exportNode = new TreeNode(export.CName);

                    foreach (string propName in export.Data.Keys)
                    {
                        exportNode.Nodes.Add($"{export.Data[propName].Name} | {export.Data[propName].TypeName}");
                    }

                    exportsNode.Nodes.Add(exportNode);
                    //propertiesNode.Nodes.Add($"{property.Name} => {property.ClassName}");
                }

                this.treeView.Nodes.Add(exportsNode);
            }
            //this.treeView.Nodes.Add($"ClassName: {file.}")
        }
    }
}
