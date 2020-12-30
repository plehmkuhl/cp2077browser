using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using ArchiveLib;
using ArchiveLib.Oodle;

namespace CP77Brow
{
    static class Program
    {
        /// <summary>
        /// Der Haupteinstiegspunkt für die Anwendung.
        /// </summary>
        [STAThread]
        static void Main()
        {
            CR2WLib.Types.CR2WValue.RegisterTypes();   
            
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            String gamePath = Properties.Settings.Default["CPGamePath"].ToString();

            try
            {
                DirectoryInfo dirInfo = new DirectoryInfo(gamePath);
                if (!dirInfo.Exists)
                    gamePath = "";
            } catch (Exception e)
            {
                Console.Error.Write(e);
                gamePath = "";
            }

            if (gamePath == "")
            {
                OpenFileDialog searchCPDialog = new OpenFileDialog();
                searchCPDialog.FileName = "Cyberpunk2077.exe";
                searchCPDialog.Title = "Select cyberpunk 2077 executable...";
                searchCPDialog.Filter = "Cyberpunk 2077|Cyberpunk2077.exe";
                searchCPDialog.RestoreDirectory = false;
                searchCPDialog.CheckFileExists = true;

                if (searchCPDialog.ShowDialog() == DialogResult.OK)
                {
                    Properties.Settings.Default["CPGamePath"] = Path.GetDirectoryName(searchCPDialog.FileName);
                    Properties.Settings.Default.Save();

                    gamePath = Properties.Settings.Default["CPGamePath"].ToString();
                }
            }

            CyberpunkGame game = new CyberpunkGame(gamePath);
            game.RegisterGamefiles();

            OodleCompression.LoadLibrary(game.GamePath);
            ArchiveManager.Initialize();

            Application.Run(new Browser());
        }
    }
}
