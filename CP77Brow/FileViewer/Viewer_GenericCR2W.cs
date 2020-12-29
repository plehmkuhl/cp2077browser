using CR2WLib;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
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
                        TreeNode exportPropNode = new TreeNode($"{export.Data[propName].Name} | {export.Data[propName].TypeName}");
                        exportNode.Nodes.Add(exportPropNode);

                        if (export.Data[propName].TypeName == "CName") {
                            BinaryReader reader = new BinaryReader(new MemoryStream(export.Data[propName].RawData));
                            exportPropNode.Text += " => " + file.CNames[reader.ReadUInt16()];
                        }

                        if (export.Data[propName].TypeName.Contains("array:"))
                        {
                            string[] propValues = null;

                            if (export.Data[propName].TypeName.Contains("raRef")) {
                                propValues = export.Data[propName].ToRaRefArray();
                            } else if (export.Data[propName].TypeName.Contains("String")) {
                                propValues = export.Data[propName].ToStringArray();
                            } else if (export.Data[propName].TypeName.Contains("Multilayer_Layer")) {
                                BinaryReader reader = new BinaryReader(new MemoryStream(export.Data[propName].RawData));
                                int elements = reader.ReadInt32();

                                for (int i=0; i < elements; i++)
                                {
                                    TreeNode layerNode = new TreeNode($"Layer {i + 1}");
                                    exportPropNode.Nodes.Add(layerNode);
                                }
                            }

                            if (propValues != null)
                            {
                                foreach(string propValue in propValues)
                                {
                                    exportPropNode.Nodes.Add(propValue);
                                }
                            }
                        } else if (export.Data[propName].TypeName.Contains("raRef:")) {
                            exportPropNode.Nodes.Add(export.Data[propName].ToRaRef());
                        } else if (export.Data[propName].TypeName.Contains("String")) {
                            exportPropNode.Nodes.Add(System.Text.Encoding.ASCII.GetString(export.Data[propName].RawData, 1, export.Data[propName].RawData.Length - 1));
                        }
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
