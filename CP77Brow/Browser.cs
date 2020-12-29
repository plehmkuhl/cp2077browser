using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using ArchiveLib;
using ArchiveLib.Tools;
using CP77Brow.FileViewer;
using CR2WLib;

namespace CP77Brow
{
    public partial class Browser : Form
    {
        private TreeNode treeNode_Unknown;
        private List<TreeNode> treeNode_Root;
        private Dictionary<string, TreeNode> treeNode_Folders;
        Dictionary<TreeNode, List<TreeNode>> nodeTree;
        Dictionary<string, TreeNode> unknownFilesType;

        public Browser()
        {
            InitializeComponent();

            this.treeNode_Unknown = new TreeNode("Missing Hashes");
            this.containerTreeView.Nodes.Add(this.treeNode_Unknown);

            this.containerTreeView.BeforeExpand += (object sender, TreeViewCancelEventArgs e) => {
                this.UpdateFileBrowserTree(e.Node, true);
            };

            this.containerTreeView.BeforeCollapse += (object sender, TreeViewCancelEventArgs e) => {
                this.UpdateFileBrowserTree(e.Node, false);
            };
        }

        private void ContainerTreeView_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            throw new NotImplementedException();
        }

        private TreeNode CreateTreeNodeFromFile(ArchiveFileInfo file)
        {
            TreeNode node = new TreeNode();
            node.Text = Path.GetFileName(file.Name);
            node.Tag = file;

            return node;
        }

        private TreeNode CreateTreeNodeFolder(string path)
        {
            string[] components = path.Split(new char []{ '\\', '/'});
            string completePath = "";

            TreeNode currentNode = null;

            foreach (string component in components)
            {
                completePath = Path.Combine(completePath, component);
                if (this.treeNode_Folders.ContainsKey(completePath)) {
                    currentNode = this.treeNode_Folders[completePath];
                } else {
                    TreeNode node = new TreeNode(component);
                    this.treeNode_Folders.Add(completePath, node);

                    if (currentNode != null)
                        this.nodeTree[currentNode].Add(node);
                    else
                        this.treeNode_Root.Add(node);

                    currentNode = node;
                    this.nodeTree.Add(node, new List<TreeNode>());
                }
            }

            return currentNode;
        }

        private void AppendFileBrowser(ArchiveFileInfo[] files)
        {
            foreach(ArchiveFileInfo f in files)
            {
                if (f.IsNamed)
                {
                    TreeNode folderNode = this.CreateTreeNodeFolder(Path.GetDirectoryName(f.Name));
                    nodeTree[folderNode].Add(this.CreateTreeNodeFromFile(f));
                }
                else {
                    try
                    {
                        ArchiveFile file = f.OpenRead();

                        try
                        {
                            CR2WFile cr2w = CR2WFile.ReadFile(file);

                            TreeNode typeNode;

                            if (!this.unknownFilesType.ContainsKey(cr2w.Exports[0].CName))
                            {
                                typeNode = new TreeNode(cr2w.Exports[0].CName);
                                this.unknownFilesType.Add(cr2w.Exports[0].CName, typeNode);
                                this.nodeTree[this.treeNode_Unknown].Add(typeNode);
                                this.nodeTree.Add(typeNode, new List<TreeNode>());
                            }
                            else
                            {
                                typeNode = this.unknownFilesType[cr2w.Exports[0].CName];
                            }

                            this.nodeTree[typeNode].Add(this.CreateTreeNodeFromFile(f));
                        } catch (Exception e) {
                            TreeNode blobNode;

                            if (!this.unknownFilesType.ContainsKey("BLOB"))
                            {
                                blobNode = new TreeNode("- BINARY FILE -");
                                this.unknownFilesType.Add("BLOB", blobNode);
                                this.nodeTree[this.treeNode_Unknown].Add(blobNode);
                                this.nodeTree.Add(blobNode, new List<TreeNode>());
                            } else {
                                blobNode = this.unknownFilesType["BLOB"];
                            }

                            this.nodeTree[blobNode].Add(this.CreateTreeNodeFromFile(f));
                        }

                        file.Close();
                    } catch (Exception e) {
                        TreeNode brokenNode;

                        if (!this.unknownFilesType.ContainsKey("BROKEN"))
                        {
                            brokenNode = new TreeNode("- BROKEN FILE -");
                            this.unknownFilesType.Add("BROKEN", brokenNode);
                            this.nodeTree[this.treeNode_Unknown].Add(brokenNode);
                            this.nodeTree.Add(brokenNode, new List<TreeNode>());
                        } else {
                            brokenNode = this.unknownFilesType["BROKEN"];
                        }

                        this.nodeTree[brokenNode].Add(this.CreateTreeNodeFromFile(f));
                    }
                }
            }
        }

        private void UpdateFileBrowser()
        {
            this.containerTreeView.BeginUpdate();

            this.containerTreeView.Nodes.Clear();

            // Add base tree
            this.containerTreeView.Nodes.AddRange(this.treeNode_Root.ToArray());
            this.containerTreeView.Nodes.Add(this.treeNode_Unknown);

            // Update visible nodes recusive 
            foreach (TreeNode n in this.containerTreeView.Nodes)
            {
                this.UpdateFileBrowserTree(n, n.IsExpanded);
            }

            this.treeNode_Unknown.Text = String.Format("Missing Hashes");
            
            this.containerTreeView.EndUpdate();

            Application.DoEvents();
        }

        private void UpdateFileBrowserTree(TreeNode node, bool expanded)
        {
            if (expanded)
            {
                this.nodeTree[node].Sort((Comparison<TreeNode>) delegate (TreeNode a, TreeNode b)
                    {
                        return a.Text.CompareTo(b.Text);
                    });

                node.Nodes.Clear();
                node.Nodes.AddRange(this.nodeTree[node].ToArray());

                foreach (TreeNode n in this.nodeTree[node])
                {
                    this.UpdateFileBrowserTree(n, n.IsExpanded);
                }
            }
            else
            {
                if (this.nodeTree.ContainsKey(node) && this.nodeTree[node].Count != 0)
                {
                    node.Nodes.Add("Dummy");
                }
                else
                {
                    node.Nodes.Clear();
                }
            }
        }

        private async void Browser_Shown(object sender, EventArgs e)
        {
            this.Visible = false;
            await this.LoadFileTree();
        }

        public void OpenFile(ArchiveFileInfo fileInfo, bool defaultTab)
        {
            try
            {
                ArchiveFile file = fileInfo.OpenRead();
                CR2WFile cr2w = CR2WFile.ReadFile(file);

                var exports = cr2w.Exports;
                foreach (CR2WExport e in exports)
                {
                    Console.WriteLine($"Got {e.CName}");
                }

                this.editorTabControl.TabPages.Clear();

                TabPage page = new TabPage(Path.GetFileName(fileInfo.Name));
                switch (cr2w.Exports[0].CName)
                {
                    case "C2dArray":
                        {
                            Viewer_C2DArray viewer = new Viewer_C2DArray(cr2w);
                            viewer.Dock = DockStyle.Fill;
                            page.Controls.Add(viewer);
                            break;
                        }

                    case "redResourceListResource":
                        {
                            Viewer_RedResourceListResource viewer = new Viewer_RedResourceListResource(cr2w);
                            viewer.Dock = DockStyle.Fill;
                            page.Controls.Add(viewer);
                            break;
                        }

                    case "worldWorldListResource":
                        {
                            Viewer_WorldWorldListResource viewer = new Viewer_WorldWorldListResource(cr2w);
                            viewer.Dock = DockStyle.Fill;
                            page.Controls.Add(viewer);
                            break;
                        }

                    default:
                        {
                            Viewer_GenericCR2W viewer = new Viewer_GenericCR2W(cr2w);
                            viewer.Dock = DockStyle.Fill;
                            page.Controls.Add(viewer);
                            break;
                        }
                }
                
                this.editorTabControl.TabPages.Add(page);
            } catch (Exception e)
            {
                Console.Error.Write(e);
            }
        }

        private void containerTreeView_DoubleClick(object sender, EventArgs e)
        {

        }

        private void containerTreeView_AfterSelect(object sender, TreeViewEventArgs e)
        {
            TreeNode selected = this.containerTreeView.SelectedNode;
            if (selected == null) return;

            ArchiveFileInfo file = (ArchiveFileInfo)selected.Tag;

            if (this.containerTreeView.ContextMenu != null)
                this.containerTreeView.ContextMenu.MenuItems.Clear();

            if (file != null)
            {
                this.OpenFile(file, true);

                ContextMenu itemMenu = new ContextMenu();
                MenuItem exportFile = new MenuItem("Export", (EventHandler)delegate (object s, EventArgs evt)
                {
                    SaveFileDialog dialog = new SaveFileDialog();
                    dialog.FileName = Path.GetFileName(file.Name);

                    if (dialog.ShowDialog() == DialogResult.OK)
                    {
                        TreeNode selected_ = this.containerTreeView.SelectedNode;
                        if (selected_ == null) return;
                        ArchiveFileInfo file_ = (ArchiveFileInfo)selected.Tag;
                        if (file_ == null)
                            return;

                        FileInfo exportFileInfo = new FileInfo(dialog.FileName);

                        FileStream targetStream = exportFileInfo.OpenWrite();
                        ArchiveFile archiveFile = file.OpenRead();

                        while(archiveFile.Position < archiveFile.Length)
                        {
                            byte[] buffer = new byte[512];
                            int read = archiveFile.Read(buffer, 0, buffer.Length);
                            targetStream.Write(buffer, 0, read);
                        }

                        targetStream.Close();
                        archiveFile.Close();

                        MessageBox.Show("Export completed!");
                    }
                });

                itemMenu.MenuItems.Add(exportFile);
                this.containerTreeView.ContextMenu = itemMenu;
            }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void changeGamepathToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private async void Browser_Load(object sender, EventArgs e)
        {

        }

        public Task LoadFileTree()
        {
            this.treeNode_Folders = new Dictionary<string, TreeNode>();
            this.treeNode_Root = new List<TreeNode>();

            this.nodeTree = new Dictionary<TreeNode, List<TreeNode>>();
            this.nodeTree.Add(this.treeNode_Unknown, new List<TreeNode>());

            this.unknownFilesType = new Dictionary<string, TreeNode>();

            LoadingArchives loaderProgress = new LoadingArchives();

            int fileCount = 0;
            int loadedCount = 0;

            // Determine file count
            foreach (Archive a in ArchiveManager.Archives)
            {
                fileCount += (int)a.FileCount;
            }

            loaderProgress.UpdateProgres(fileCount, 0);

            if (this.Visible)
            {
                loaderProgress.Parent = this;
                loaderProgress.Show();
            }
            else
            {
                loaderProgress.Show();
            }

            return Task.Run(() =>
            {
                // Start loading
                SynchronizedCollection<ArchiveFileInfo> appendFiles = new SynchronizedCollection<ArchiveFileInfo>();

                List<Task> loadTasks = new List<Task>();
                IAsyncResult updateCall = null;

                foreach (Archive a in ArchiveManager.Archives)
                {
                    Console.WriteLine("Starting file listing for: " + a.File.Name);

                    loadTasks.Add(a.ListFilesAsync((ArchiveFileInfo file) =>
                    {
                        appendFiles.Add(file);
                        loadedCount++;

                        if (appendFiles.Count >= 100)
                        {
                            lock (appendFiles.SyncRoot)
                            {
                                ArchiveFileInfo[] files = appendFiles.ToArray();
                                appendFiles.Clear();

                                this.AppendFileBrowser(files);
                                loaderProgress.UpdateProgres(fileCount, loadedCount);

                                /*if (updateCall == null || updateCall.IsCompleted)
                                {
                                    updateCall = this.BeginInvoke((MethodInvoker)delegate ()
                                    {
                                        this.UpdateFileBrowser();
                                    });
                                }*/
                            }
                        }
                    }));
                }

                Task.WaitAll(loadTasks.ToArray());

                loaderProgress.UpdateProgres(fileCount, loadedCount);

                this.AppendFileBrowser(appendFiles.ToArray());

                this.BeginInvoke((MethodInvoker)delegate ()
                {
                    this.UpdateFileBrowser();
                    this.Show();
                }).AsyncWaitHandle.WaitOne(); ;

                loaderProgress.BeginInvoke((MethodInvoker)delegate () { loaderProgress.Close(); });

                Console.WriteLine("All files added");
            });
        }

        private void decodeHashesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // This will take a long time
            LoadingArchives loaderProgress = new LoadingArchives();

            int fileCount = 0;
            int doneCount = 0;

            // Determine file count
            foreach (Archive a in ArchiveManager.Archives)
            {
                fileCount += (int)a.FileCount;
            }

            loaderProgress.UpdateProgres(fileCount, 0);
            loaderProgress.Show();

            //Dictionary<ulong, string> foundHashes = new Dictionary<ulong, string>();
            object FileLock = new Object();

            Task.Run(() =>
            {
                List<Task> loadTasks = new List<Task>();

                foreach (Archive a in ArchiveManager.Archives)
                {
                    Console.WriteLine("Starting file listing for: " + a.File.Name);

                    loadTasks.Add(a.ListFilesAsync((ArchiveFileInfo file) =>
                    {
                        try
                        {
                            ArchiveFile af = file.OpenRead();

                            try
                            {
                                CR2WFile cr2w = CR2WFile.ReadFile(af);

                                foreach (CR2WImport import in cr2w.Imports)
                                {
                                    ulong hash = FNV1A64HashAlgorithm.HashString(import.Path);
                                    if (ArchiveManager.ResolveFileHash(hash) == "")
                                    {
                                        ArchiveManager.RegisterFilePath(import.Path);
                                        Console.WriteLine($"Found missing hash from imports: {import.Path},{hash}");
                                    }
                                }

                                foreach (CR2WExport export in cr2w.Exports)
                                {
                                    if (export.Data.ContainsKey("resolvedDependencies") && export.Data["resolvedDependencies"].TypeName == "array:raRef:CResource")
                                    {
                                        string[] paths = export.Data["resolvedDependencies"].ToRaRefArray();

                                        foreach (string path in paths)
                                        {
                                            ulong hash = FNV1A64HashAlgorithm.HashString(path);
                                            if (ArchiveManager.ResolveFileHash(hash) == "")
                                            {
                                                ArchiveManager.RegisterFilePath(path);
                                                Console.WriteLine($"Found missing hash from resolved dependencies: {path},{hash}");
                                            }
                                        }
                                    }

                                    if (export.Data.ContainsKey("dependencies") && export.Data["dependencies"].TypeName == "array:raRef:CResource")
                                    {
                                        string[] paths = export.Data["dependencies"].ToRaRefArray();

                                        foreach (string path in paths)
                                        {
                                            ulong hash = FNV1A64HashAlgorithm.HashString(path);
                                            if (ArchiveManager.ResolveFileHash(hash) == "")
                                            {
                                                ArchiveManager.RegisterFilePath(path);
                                                Console.WriteLine($"Found missing hash from cooked dependency: {path},{hash}");
                                            }
                                        }
                                    }

                                    if (export.CName == "appearanceAppearanceDefinition")
                                    {
                                        try
                                        {
                                            BinaryReader reader = new BinaryReader(new MemoryStream(export.Data["name"].RawData));
                                            string appearanceName = cr2w.CNames[reader.ReadUInt16()];

                                            if (!cr2w.Exports[0].Data.ContainsKey("commonCookData"))
                                                continue;

                                            // Get name of common cook data
                                            string baseName = Path.GetFileNameWithoutExtension(cr2w.Exports[0].Data["commonCookData"].ToRaRef());
                                            baseName = baseName.Substring(7, baseName.Length - 7);

                                            string cookedName = $"base\\cookedappearances\\{baseName}_{appearanceName}.cookedapp";

                                            // Search additional cooked data
                                            ulong hash = FNV1A64HashAlgorithm.HashString(cookedName);
                                            if (ArchiveManager.ResolveFileHash(hash) == "")
                                            {
                                                ArchiveManager.RegisterFilePath(cookedName);
                                                Console.WriteLine($"Found missing hash from coocked appearances: {cookedName},{hash}");
                                            }
                                        } catch (Exception) {}
                                    }
                                }
                            } catch (Exception) {}

                            af.Close();
                        } catch (Exception) {}

                        doneCount++;
                        if ((doneCount % 100) == 0)
                            loaderProgress.UpdateProgres(fileCount, doneCount);
                    }));
                }

                Task.WaitAll(loadTasks.ToArray());

                loaderProgress.UpdateProgres(fileCount, doneCount);
                loaderProgress.BeginInvoke((MethodInvoker)delegate () { loaderProgress.Close(); });

                this.LoadFileTree();
            });
        }

        private void byPathToolStripMenuItem_Click(object sender, EventArgs e)
        {
            FileNameSearch searchInput = new FileNameSearch();

            if (searchInput.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    ArchiveManager.SearchFile(searchInput.FileName);
                    Console.WriteLine($"File {searchInput.FileName} found!");
                } catch (FileNotFoundException)
                {
                    Console.Error.WriteLine($"File {searchInput.FileName} not found!");
                }
            }
        }

        private void exportHashesToolStripMenuItem_Click(object sender, EventArgs e)
        {
            LoadingArchives loaderProgress = new LoadingArchives();

            SaveFileDialog saveHashListDialog = new SaveFileDialog();
            saveHashListDialog.Filter = "Hashlist (*.txt) | *.txt";
            if (saveHashListDialog.ShowDialog() != DialogResult.OK) return;

            FileStream outputFile = new FileStream(saveHashListDialog.FileName, FileMode.OpenOrCreate);
            outputFile.SetLength(0);

            StreamWriter hashWriter = new StreamWriter(outputFile);

            int fileCount = 0;
            int loadedCount = 0;

            // Determine file count
            foreach (Archive a in ArchiveManager.Archives)
            {
                fileCount += (int)a.FileCount;
            }

            loaderProgress.UpdateProgres(fileCount, 0);
            loaderProgress.Show();

            Task.Run(() =>
            {
                // Start loading
                SynchronizedCollection<ArchiveFileInfo> appendFiles = new SynchronizedCollection<ArchiveFileInfo>();

                List<Task> loadTasks = new List<Task>();

                foreach (Archive a in ArchiveManager.Archives)
                {
                    Console.WriteLine("Starting file listing for: " + a.File.Name);

                    loadTasks.Add(a.ListFilesAsync((ArchiveFileInfo file) =>
                    {
                        lock(hashWriter)
                        {
                            hashWriter.WriteLine(file.NameHash.ToString());
                        }
                    }));
                }

                Task.WaitAll(loadTasks.ToArray());

                outputFile.Close();

                loaderProgress.UpdateProgres(fileCount, loadedCount);
                loaderProgress.BeginInvoke((MethodInvoker)delegate () { loaderProgress.Close(); });

                Console.WriteLine("Hashes exported");
            });
        }
    }
}
