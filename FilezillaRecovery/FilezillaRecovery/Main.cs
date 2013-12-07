using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using Microsoft.Win32;
using System.Xml;

namespace FileZilla_Data_Extractor
{


    public partial class frmMain : Form
    {
        //Use for storing data that will be written to CSV file
        List<SharedProcess.CsvRow> csvContent = new List<SharedProcess.CsvRow>();

        public frmMain()
        {
            InitializeComponent();
        }

        //Extract and display data for .reg file
        private String extractText(String line, String Value)
        {
            int start = 0, len = 0;
            start = line.IndexOf("=") + 2;
            len = line.LastIndexOf('"') - start;
            String returned = "";

            if (Value.Equals("Password"))
            {
                txtResults.AppendText(Value + ": " + SharedProcess.decodePW(line.Substring(start, len)) + "\n");
                returned = SharedProcess.decodePW(line.Substring(start, len));
            }
            else
            {
                txtResults.AppendText(Value + ": " + line.Substring(start, len) + "\n");
                returned = line.Substring(start, len);
            }
            return returned;
        }

        private void frmMain_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent(DataFormats.FileDrop, false))
                e.Effect = DragDropEffects.All;
        }

        private void frmMain_DragDrop(object sender, DragEventArgs e)
        {
            txtPath.Clear();
            txtResults.Clear();
            csvContent = new List<SharedProcess.CsvRow>();
            btnExport.Enabled = false;
            string[] fileList = e.Data.GetData(DataFormats.FileDrop) as string[];
            if (!txtPath.Text.Equals(""))
            {
                txtPath.Text += ";";
            }
            if (fileList.Length == 1)
            {
                txtPath.Text += String.Format("{0}", fileList[0]);
            }
            else
            {
                for (int i = 0; i < fileList.Length; i++)
                {
                    if (i != fileList.Length - 1)
                    {
                        txtPath.Text += String.Format("{0}{1}", fileList[i], ";");
                    }
                    else
                    {
                        txtPath.Text += String.Format("{0}", fileList[i]);
                    }
                }
            }

            string[] words = txtPath.Text.Split(';');

            foreach (string word in words)
            {
                try
                {
                    XDocument doc = XDocument.Load(word);
                    txtResults.AppendText("------------------------------------------------------\n");
                    txtResults.AppendText("From file: ");
                    txtResults.AppendText("'" + word + "'\n");
                    txtResults.AppendText("------------------------------------------------------\n\n");
                    var query = from d in doc.Root.Descendants("Server") select d;
                    bool bEmptyValue = true;
                    bool hflag = false;
                    bool uflag = false;
                    SharedProcess.CsvRow row = new SharedProcess.CsvRow();

                    foreach (var q in query)
                    {
                        row = new SharedProcess.CsvRow();
                        bEmptyValue = false;
                        try
                        {
                            uflag = false;
                            String hostval = q.Element("Host").Value;
                            hflag = true;
                            row.Add(hostval);
                            txtResults.AppendText("Host URL: ");
                            txtResults.AppendText("'" + hostval + "'\n");

                            txtResults.AppendText("Username: " + q.Element("User").Value + '\n');
                            uflag = true;
                            row.Add(q.Element("User").Value);
                            txtResults.AppendText("Password: " + q.Element("Pass").Value + '\n');
                            row.Add(q.Element("Pass").Value);
                            csvContent.Add(row);
                            txtResults.AppendText("\n");

                        }
                        catch (Exception) //Handle Empty password and username for ver3 and handle xml file for 2.X
                        {
                            if (hflag == false)
                            {
                                XmlDocument doc1 = new XmlDocument();
                                doc1.Load(word);
                                System.Xml.XmlNodeList servers = doc1.SelectNodes("//Server");
                                foreach (XmlNode server in servers)
                                {
                                    txtResults.AppendText("Host: " + server.Attributes["Host"].Value + "\n");
                                    row.Add(server.Attributes["Host"].Value);
                                    txtResults.AppendText("Username: " + server.Attributes["User"].Value + "\n");
                                    row.Add(server.Attributes["User"].Value);
                                    String password = SharedProcess.decodePW(server.Attributes["Pass"].Value);
                                    if (password.Equals(""))
                                    {
                                        txtResults.AppendText("Password: Empty \n\n");
                                        row.Add("");
                                    }
                                    else
                                    {
                                        txtResults.AppendText("Password: " + password + "\n\n");
                                        row.Add(password);
                                    }
                                    csvContent.Add(row);
                                    row = new SharedProcess.CsvRow();
                                }
                                break;
                            }
                            else
                            {
                                if (uflag == false)
                                {
                                    txtResults.AppendText("Username: Empty\n");
                                    row.Add("");
                                    txtResults.AppendText("Password: Empty\n");
                                    row.Add("");
                                    txtResults.AppendText("\n");
                                }
                                else
                                {
                                    txtResults.AppendText("Password: Empty\n");
                                    row.Add("");
                                    txtResults.AppendText("\n");
                                }
                                csvContent.Add(row);
                                row = new SharedProcess.CsvRow();
                            }
                        }
                        finally
                        {
                            btnExport.Enabled = true;
                        }
                    }
                    if (bEmptyValue)
                    {
                        txtResults.AppendText("This file does not contain any filezilla host, user or password.\n\n");
                    }
                }
                catch (System.Xml.XmlException) //Handle the case of .reg file and not valid file format
                {
                    if (Path.GetExtension(word).Equals(".reg"))
                    {
                        try
                        {
                            string line;
                            System.IO.StreamReader file = new System.IO.StreamReader(word);
                            txtResults.AppendText("------------------------------------------------------\n");
                            txtResults.AppendText("From file: ");
                            txtResults.AppendText("'" + word + "'\n");
                            txtResults.AppendText("------------------------------------------------------\n\n");

                            while ((line = file.ReadLine()) != null)
                            {
                                SharedProcess.CsvRow row = new SharedProcess.CsvRow();
                                if (line.Contains("Server.Host"))
                                {
                                    row.Add(extractText(line, "Host"));
                                    line = file.ReadLine();
                                    line = file.ReadLine();
                                    if (line.Contains("\"\""))
                                    {
                                        txtResults.AppendText("User: Empty\n");
                                        txtResults.AppendText("Password: Empty\n");

                                    }
                                    else
                                    {
                                        row.Add(extractText(line, "User"));
                                        line = file.ReadLine();
                                        if (line.Contains("\"\""))
                                        {
                                            txtResults.AppendText("Password: Empty\n");
                                            row.Add("");
                                        }
                                        else
                                        {
                                            row.Add(extractText(line, "Password"));
                                            txtResults.AppendText("\n");
                                        }
                                    }
                                    csvContent.Add(row);
                                    btnExport.Enabled = true;
                                }
                            }
                        }
                        catch (Exception)
                        {
                            txtResults.AppendText("The file could not be read");
                        }
                    }
                    else
                    {
                        txtResults.AppendText("Invalid file format\n");
                    }
                }
            }
        }

        private void btnExtract_Click(object sender, EventArgs e)
        {
            txtResults.Clear();

            csvContent = new List<SharedProcess.CsvRow>();
            try
            {
                string keyname = txtPath.Text;
                string pw = "";
                string host = "";
                SharedProcess.CsvRow row = new SharedProcess.CsvRow();
                //string keyname = "HKEY_USERS\\Sandbox_BK_DefaultBox\\user\\current\\software\\FileZilla\\Recent Servers\\Server 1";

                host = Registry.GetValue(keyname, "Last Server Host", "No Server Exist").ToString();
                if (host.Equals("No Server Exist"))
                {
                    host = Registry.GetValue(keyname, "Server.Host", "No Server Exist").ToString();
                }

                if (!host.Equals("No Server Exist"))
                {
                    string user = Registry.GetValue(keyname, "Last Server User", "No Username Exist").ToString();
                    if (user.Equals("No Username Exist"))
                    {
                        user = Registry.GetValue(keyname, "Server.User", "No Username Exist").ToString();
                    }

                    if (!user.Equals("No Username Exist"))
                    {
                        pw = Registry.GetValue(keyname, "Last Server Pass", "No Password Exist").ToString();
                        if (pw.Equals("No Password Exist"))
                        {
                            pw = Registry.GetValue(keyname, "Server.Pass", "No Password Exist").ToString();
                        }

                        row.Add(host);
                        row.Add(user);
                        row.Add(SharedProcess.decodePW(pw));
                        csvContent.Add(row);

                        txtResults.AppendText("Host : " + host + "\nUser : " + user + "\n" + "Password : " + SharedProcess.decodePW(pw) + "\n");
                        btnExport.Enabled = true;
                    }
                    else
                    {
                        row.Add(host);
                        row.Add(user);
                        row.Add("");
                        txtResults.AppendText("Host : " + host + "\nUser : " + user + "\n" + "Password : No password avaliable\n");
                        btnExport.Enabled = true;
                    }
                }
                else
                {
                    txtResults.AppendText("No valid information avaliable");
                }

            }
            catch (Exception)
            {
                txtResults.AppendText("This is not a valid registry key\n");
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (saveFileDialog1.ShowDialog() == DialogResult.OK)
            {
                using (SharedProcess.CsvFileWriter writer = new SharedProcess.CsvFileWriter(saveFileDialog1.FileName))
                {
                    foreach (SharedProcess.CsvRow r in csvContent)
                    {
                        writer.WriteRow(r);
                    }
                }
            }
        }
    }
}