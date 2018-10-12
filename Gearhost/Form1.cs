using System;
using System.Linq;
using System.IO;
using System.Windows.Forms;
using System.Web.Script.Serialization;

namespace Gearhost
{
    public partial class Form1 : Form
    {
        string downloadsPath;

        public Form1()
        {
            InitializeComponent();
        }

        private bool ExecBackup()
        {
            string apiKey = txtAPIKey.Text;
            string dbName = txtDB.Text;

            FolderBrowserDialog folderDlg = new FolderBrowserDialog();

            folderDlg.ShowNewFolderButton = true;

            DialogResult result = folderDlg.ShowDialog();

            if (result == DialogResult.OK)
            {
                 downloadsPath = folderDlg.SelectedPath.ToString();
            }

            using (var webClient = new GearHostWebClient())
            {
                webClient.Headers.Add("Authorization", string.Format("bearer {0}", apiKey));
                DatabasesDTO dto = null;
                try
                {
                    var databasesJson = webClient.DownloadString("https://api.gearhost.com/v1/databases");
                    dto = new JavaScriptSerializer().Deserialize<DatabasesDTO>(databasesJson);
                }
                catch (Exception)
                {
                    txtLog.Text += "Error downloading API data. Check your API key \r\n";
                    return false;
                }

                var database = dto.databases.FirstOrDefault(d => string.Compare(d.name, dbName, StringComparison.OrdinalIgnoreCase) == 0);
                if (database == null)
                {
                    txtLog.Text += "Database not found. Check your database name. \r\n";
                    return false;
                }
                txtLog.Text += "Database found. Executing backup... \r\n";
                string localFileName = dbName + "_" + DateTime.Now.ToString("yyyy-dd-M--HH-mm-ss") + ".zip";
                string localPath = Path.Combine(downloadsPath, localFileName);
                txtLog.Text += localPath.ToString() + "\r\n";
                
                webClient.DownloadFile(string.Format("https://api.gearhost.com/v1/databases/{0}/backup", database.id), localPath);
                
            }

            return true;
}


        private void button1_Click(object sender, EventArgs e)
        {
            txtLog.Text = "";
            try
            {
                if (!ExecBackup())
                {
                    txtLog.Text += "Aborting... \r\n";
                }
                else
                {
                    txtLog.Text += "Backup successfully downloaded. \r\n";
                    MessageBox.Show("Done!.", "Backup Completed!", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
