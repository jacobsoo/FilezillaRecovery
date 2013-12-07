using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using System.Xml.Linq;
using System.Xml;
using System.Runtime.InteropServices;

namespace FileZilla_Data_Extractor
{
    static class Program
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            //string[] testargs = { "-f", @"C:\Users\BK\Desktop\Filezilla Application\FileZilla.xml;C:\Users\BK\Desktop\Filezilla Application\Filezilla2.reg", "-o", @"C:\Users\BK\Desktop\Filezilla Application\testoutput.csv" };
            //args = testargs;
            //Run GUI Version
            if (args.Length == 0)
            {
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                Application.Run(new frmMain());

            }

            else
            {
                List<SharedProcess.CsvRow> csvContent = new List<SharedProcess.CsvRow>();
                string[] inputfile = new string[1];
                string outputfile = "";
                string keyname = "";
                //string[] testargs = {"-f",@"C:\Users\BK\Desktop\Filezilla Application\FileZilla.xml;C:\Users\BK\Desktop\Filezilla Application\Filezilla2.reg;C:\Users\BK\Desktop\Filezilla Application\Filezilla3.reg","-o",@"C:\Users\BK\Desktop\Filezilla Application\"};
                // args = testargs;
                //string[] testargs = { "-r", "HKEY_USERS\\Sandbox_BK_DefaultBox\\user\\current\\software\\FileZilla\\Recent Servers\\Server 1", "-o", @"C:\Users\BK\Desktop\Filezilla Application\testoutput.csv" };
              // args = testargs;
                if (args.Length != 4 || (args[0] == "-f" && args[2] != "-o") || (args[0] == "-o" && args[2] != "-f")
                    || (args[0] == "-r" && args[2] != "-o") || (args[0] == "-o" && args[2] != "-r"))
                {
                    const string message = "Error in format, Enter as in format below:\n" +
                       "Eg. FileZilla Data Extractor.exe -f [filename1];[filename2]...[filenameN] -o [outputfile]\n" +
                       "Eg. FileZilla Data Extractor.exe -o [outputfilepath] -f [filename1];[filename2]...[filenameN]\n" +
                       "Eg. FileZilla Data Extractor -r [registrykey] -o [outputfile]";

                    MessageBox.Show(message, "Error in format", MessageBoxButtons.OK, MessageBoxIcon.Error);


                }

                else
                {
                    //If parameter entered is registry
                    if ((args[0] == "-r" && args[2] == "-o") || (args[0] == "-o" && args[2] == "-r"))
                    {


                        if (args[0] == "-r" && args[2] == "-o")
                        {
                            keyname = args[1];
                            outputfile = args[3];
                        }

                        else
                        {
                            keyname = args[3];
                            outputfile = args[1];
                        }

                        //Check output file path
                        string directory;
                        if (File.Exists(outputfile))
                        {
                            var result = MessageBox.Show("File exist!, Overwrite ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                            if (result != DialogResult.Yes)
                            {
                                outputfile = "1";
                            }


                        }

                        else
                        {
                            if (outputfile.EndsWith(".csv"))
                            {
                                int index = outputfile.LastIndexOf('\\');
                                directory = outputfile.Substring(0, index);
                                if (!Directory.Exists(directory))
                                {
                                    outputfile = "";
                                }
                            }

                            else
                            {
                                outputfile = "2";
                            }
                        }

                        //
                        if (outputfile != "" && outputfile != "1" && outputfile != "2")
                        {
                            csvContent = new List<SharedProcess.CsvRow>();
                            try
                            {
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
                                    }

                                    else
                                    {
                                        row.Add(host);
                                        row.Add(user);
                                        row.Add("");

                                    }
                                }

                                else
                                {
                                    MessageBox.Show("No valid information avaliable", "Empty", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                                    outputfile = "3";
                                }

                            }

                            catch (Exception)
                            {
                                MessageBox.Show("Invalid registry key", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            }

                        }
                    
                    }

                    
                    //if parameter is an file
                    else
                    {
                        Boolean firstLoop = true;
                        string[] temp = new string[1];
                        temp[0] = null;

                        for (int i = 0; i < 4; i++)
                        {
                            if (args[i].Equals("-f") && !args[i + 1].Equals("-o"))
                            {
                                inputfile = args[i + 1].Split(';');

                                foreach (string input in inputfile)
                                {
                                    if (File.Exists(input))
                                    {
                                        if (firstLoop)
                                        {
                                            temp[0] = input;
                                            firstLoop = false;
                                        }

                                        else
                                        {
                                            string[] hold = new string[temp.Length + 1];
                                            Array.Copy(temp, 0, hold, 0, temp.Length);
                                            hold[temp.Length] = input;
                                            temp = null;
                                            temp = hold;
                                            hold = null;

                                        }
                                    }

                                }
                                inputfile = temp;
                                i++;

                            }

                            else if (args[i].Equals("-o") && !args[i + 1].Equals("-f"))
                            {
                                string directory;
                                if (File.Exists(args[i + 1]))
                                {
                                    var result = MessageBox.Show("File exist!, Overwrite ?", "Warning", MessageBoxButtons.YesNo, MessageBoxIcon.Exclamation);
                                    if (result == DialogResult.Yes)
                                    {
                                        outputfile = args[i + 1];
                                    }
                                    else
                                    {
                                        outputfile = "1";
                                    }

                                }

                                else
                                {
                                    if (args[i + 1].EndsWith(".csv"))
                                    {
                                        int index = args[i + 1].LastIndexOf('\\');
                                        directory = args[i + 1].Substring(0, index);

                                        if (Directory.Exists(directory))
                                        {
                                            outputfile = args[i + 1];
                                        }

                                        else
                                        {
                                            outputfile = "";
                                        }

                                    }

                                    else
                                    {
                                        outputfile = "2";
                                    }
                                }
                            }


                        }

                        if (inputfile[0] == "")
                        {
                            MessageBox.Show("Invalid input(s)", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                            outputfile = "3";
                        }

                        if (inputfile[0] != ""&&outputfile != "" && outputfile != "1" && outputfile != "2")
                        {
                            foreach (string word in inputfile)
                            {
                                try
                                {

                                    XDocument doc = XDocument.Load(word);
                                    var query = from d in doc.Root.Descendants("Server") select d;
                                    bool emptyvalue = true;
                                    bool except = false;
                                    bool hflag = false;
                                    bool uflag = false;
                                    SharedProcess.CsvRow row = new SharedProcess.CsvRow();

                                    foreach (var q in query)
                                    {
                                        row = new SharedProcess.CsvRow();
                                        emptyvalue = false;
                                        try
                                        {
                                            uflag = false;
                                            String hostval = q.Element("Host").Value;
                                            hflag = true;
                                            row.Add(hostval);
                                            uflag = true;
                                            row.Add(q.Element("User").Value);
                                            row.Add(q.Element("Pass").Value);
                                            csvContent.Add(row);

                                        }



                                        catch (Exception)//Handle Empty password and username for ver3 and handle xml file for 2.X
                                        {
                                            except = true;
                                            if (hflag == false)
                                            {
                                                XmlDocument doc1 = new XmlDocument();
                                                doc1.Load(word);
                                                System.Xml.XmlNodeList servers = doc1.SelectNodes("//Server");
                                                foreach (XmlNode server in servers)
                                                {
                                                    row.Add(server.Attributes["Host"].Value);
                                                    row.Add(server.Attributes["User"].Value);
                                                    String password = SharedProcess.decodePW(server.Attributes["Pass"].Value);
                                                    if (password.Equals(""))
                                                    {
                                                        row.Add("");
                                                    }

                                                    else
                                                    {
                                                        row.Add(password);
                                                    }

                                                    row.Add(word);
                                                    csvContent.Add(row);
                                                    row = new SharedProcess.CsvRow();
                                                }

                                                break;

                                            }

                                            else
                                            {

                                                if (uflag == false)
                                                {
                                                    row.Add("");
                                                    row.Add("");
                                                }

                                                else
                                                {
                                                    row.Add("");
                                                }

                                                row.Add(word);
                                                csvContent.Add(row);
                                                row = new SharedProcess.CsvRow();
                                            }





                                        }
                                    }
                                }

                                catch (System.Xml.XmlException)//Handle the case of .reg file and not valid file format
                                {
                                    if (Path.GetExtension(word).Equals(".reg"))
                                    {
                                        try
                                        {
                                            string line;
                                            System.IO.StreamReader file = new System.IO.StreamReader(word);

                                            while ((line = file.ReadLine()) != null)
                                            {
                                                SharedProcess.CsvRow row = new SharedProcess.CsvRow();
                                                if (line.Contains("Server.Host"))
                                                {
                                                    row.Add(SharedProcess.extractText(line, "Host"));
                                                    line = file.ReadLine();
                                                    line = file.ReadLine();
                                                    if (line.Contains("\"\""))
                                                    {
                                                        //richTextBox1.AppendText("User: Empty\n");
                                                        //richTextBox1.AppendText("Password: Empty\n");

                                                    }

                                                    else
                                                    {
                                                        row.Add(SharedProcess.extractText(line, "User"));
                                                        line = file.ReadLine();
                                                        if (line.Contains("\"\""))
                                                        {
                                                            //richTextBox1.AppendText("Password: Empty\n");
                                                            row.Add("");
                                                        }

                                                        else
                                                        {
                                                            row.Add(SharedProcess.extractText(line, "Password"));
                                                        }

                                                    }

                                                    row.Add(word);
                                                    csvContent.Add(row);

                                                }
                                            }
                                        }
                                        catch (Exception)
                                        {
                                            MessageBox.Show("File: "+word+" could be read", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                              
                                        }
                                    }
                                    else
                                    {
                                        MessageBox.Show("File: " + word + " invalid file format", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                              
                                    }
                                }
                            }

                        }




                    }

                    if (outputfile != "" && outputfile != "1"&& outputfile != "2" && outputfile!="3" )
                    {
                        SharedProcess.writeFile(csvContent, outputfile);
                    }

                    else
                    {
                        if (outputfile == "")
                        {
                            MessageBox.Show("Invalid output", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        else 
                        {
                            if (outputfile == "1")
                            {
                                MessageBox.Show("Please enter a different output file name", "Renter", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                
                            }

                            else
                            {
                                if(outputfile == "2")
                                MessageBox.Show("Invalid output file format", "File format", MessageBoxButtons.OK, MessageBoxIcon.Exclamation);
                               
                            }
                        }

                       
                    }




                }
            }

        }
    }
}
