using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace FileZilla_Data_Extractor
{
    class SharedProcess
    {
        public class CsvRow : List<string>
        {
            public string LineText { get; set; }
        }

        public class CsvFileWriter : StreamWriter
        {
            public CsvFileWriter(Stream stream)
                : base(stream)
            {
            }

            public CsvFileWriter(string filename)
                : base(filename)
            {
            }

            /// <summary>
            /// Writes a single row to a CSV file.
            /// </summary>
            /// <param name="row">The row to be written</param>
            public void WriteRow(CsvRow row)
            {
                StringBuilder builder = new StringBuilder();
                bool firstColumn = true;
                foreach (string value in row)
                {
                    // Add separator if this isn't the first value
                    if (!firstColumn)
                        builder.Append(',');
                    // Implement special handling for values that contain comma or quote
                    // Enclose in quotes and double up any double quotes
                    if (value.IndexOfAny(new char[] { '"', ',' }) != -1)
                        builder.AppendFormat("\"{0}\"", value.Replace("\"", "\"\""));
                    else
                        builder.Append(value);
                    firstColumn = false;
                }
                row.LineText = builder.ToString();
                WriteLine(row.LineText);
            }
        }

        //Decode password for filezilla version 2.X
        public static String decodePW(String pw)
        {
            string en_key = "FILEZILLA1234567890ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            String decodepw = "";
            int numChar = pw.Length / 3;
            int en_keypos = numChar % en_key.Length;


            for (int i = 0, j = 0; j < numChar; i = i + 3)
            {

                char l = (char)Convert.ToInt32(pw.Substring(i, 3));
                char m = en_key[(j + en_keypos) % en_key.Length];
                decodepw += (char)(l ^ m);
                j++;
            }
            return decodepw;
        }

        //Extract and display data for .reg file
        public static String extractText(String line, String Value)
        {
            int start = 0, len = 0;
            start = line.IndexOf("=") + 2;
            len = line.LastIndexOf('"') - start;
            String returned = "";
            if (Value.Equals("Password"))
            {
                returned = decodePW(line.Substring(start, len));
            }
            else
            {
                returned = line.Substring(start, len);
            }

            return returned;
        }

        public static void writeFile(List<SharedProcess.CsvRow> csvContent, string fileName)
        {
            using (SharedProcess.CsvFileWriter writer = new SharedProcess.CsvFileWriter(fileName))
            {
                foreach (SharedProcess.CsvRow r in csvContent)
                {
                    writer.WriteRow(r);
                }
            }
        }
    }
}
