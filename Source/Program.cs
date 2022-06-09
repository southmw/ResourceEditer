using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Text;
using Newtonsoft;
using Newtonsoft.Json;



namespace ResourceEditer
{
    internal class Program
    {
        private static string filename = string.Empty;
        private static int CodePage_EUC_KR = 51949;
        private static List<List<string>> rowData = new List<List<string>>();

        private static void Main(string[] args)
        {
            Console.WriteLine("Resource Editer Start");
            Console.WriteLine("==================================================");

            if (GetParameter(args))
            {
                Console.WriteLine("File Process");
                DataTable dataTable = new DataTable();

                FileProc(filename);
                List<string> hearderRow = rowData[0];

                rowData.RemoveAt(0);
                
                foreach (string header in hearderRow)
                {
                    dataTable.Columns.Add(new DataColumn(header, typeof(string)));
                }

                DataRow dataRow = null;

                foreach (List<string> item in rowData)
                {
                    dataRow = dataTable.NewRow();

                    for (int i = 0; i < item.Count; i++)
                    {
                        dataRow[hearderRow[i]] = item[i];

                        if (i == 5)
                        {
                            Dictionary<string, string> tags = JsonConvert.DeserializeObject<Dictionary<string, string>>(item[i]);

                            foreach (KeyValuePair<string, string> tag in tags)
                            {
                                if (!dataTable.Columns.Contains(tag.Key))
                                {
                                    dataTable.Columns.Add(new DataColumn(tag.Key, typeof(string)));
                                }
                                dataRow[tag.Key] = tag.Value;
                            }
                        }
                    }
                    dataTable.Rows.Add(dataRow);
                }

                PrintData(dataTable);
                SaveFile("Result.csv", dataTable);
            }

            Console.WriteLine("==================================================");
            Console.WriteLine("Resource Editer Exit");
        }

        private static void PrintData(DataTable dataTable)
        {
            Console.WriteLine("==================================================");
            foreach (DataColumn columns in dataTable.Columns)
            {
                Console.Write($"\t{columns.ColumnName}");
            }
            Console.WriteLine();
            Console.WriteLine("==================================================");

            foreach (DataRow row in dataTable.Rows)
            {
                string[] items = Array.ConvertAll(row.ItemArray, x => x.ToString());

                foreach (string item in items)
                {
                    Console.Write($"\t{item}");
                }
                Console.WriteLine();
            }

            Console.WriteLine();

            Console.WriteLine("==================================================");
        }

        private static void SaveFile(string filename, DataTable dataTable)
        {
            StreamWriter sw = null;

            using (sw = new StreamWriter(new FileStream(filename, FileMode.Create, FileAccess.ReadWrite, FileShare.ReadWrite), GetEncoding(CodePage_EUC_KR)))
            {
                foreach (DataColumn columns in dataTable.Columns)
                {
                    sw.Write($@"""{columns.ColumnName}"",");
                }
                sw.WriteLine();

                foreach (DataRow row in dataTable.Rows)
                {
                    string[] items = Array.ConvertAll(row.ItemArray, x => x.ToString());

                    foreach (string item in items)
                    {
                        sw.Write($@"""{item.Replace(@"""", @"""""")}"",");
                    }
                    sw.WriteLine();
                }
                sw.Close();
                sw.Dispose();
            }
            sw = null;
        }

        private static void FileProc(string filename)
        {
            StreamReader sr = null;

            using (sr = new StreamReader(filename, GetEncoding(CodePage_EUC_KR)))
            {
                int lineNo = 0;
                while (!sr.EndOfStream)
                {
                    string line = sr.ReadLine();
                    FileProcess(line, ++lineNo);
                }
                sr.Close();
                sr.Dispose();
            }

            sr = null;
        }

        private static void FileProcess(string line,int lineno)
        {
            List<string> lstTemp = new List<string>();

            if (lineno == 2)
            {
                string[] words = line.Split(',');

                List<string> rowtempData= new List<string>();
                foreach (string word in words)
                {
                    if (!string.IsNullOrWhiteSpace(word))
                    {
                        lstTemp.Add(word);
                    }
                }

                rowData.Add(lstTemp);
            }
            else if(lineno > 2)
            {
                char[] chars = line.ToCharArray();
                string temp = string.Empty;
                bool startstr = false;
                bool startjson = false;

                foreach (char chr in chars)
                {
                    switch(chr)
                    {
                        case ',':
                            if (startjson)
                            {
                                temp += chr;
                            }
                            else
                            {
                                // Console.WriteLine(temp);
                                lstTemp.Add(temp.Replace(@"""""", @""""));
                                temp = string.Empty;
                            }
                            break;
                        case '\"':
                            if (startjson)
                            {
                                temp += chr;
                            }
                            else
                            {
                                startstr = !startstr;
                            }
                            break;
                        case '{':
                            startjson = true;
                            temp += chr;
                            break;
                        case '}':
                            startjson = false;
                            temp += chr;
                            break;
                        default:
                            if (startstr)
                            {
                                temp += chr;
                            }
                            break;
                    }
                }

                if (!string.IsNullOrWhiteSpace(temp))
                {
                    // Console.WriteLine(temp);
                    lstTemp.Add(temp.Replace(@"""""", @""""));
                    temp = string.Empty;
                }

                rowData.Add(lstTemp);
            }
            else
            {

            }
        }

        private static Encoding GetEncoding(int codepage)
        {
            Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
            return Encoding.GetEncoding(codepage);
        }

        private static bool GetParameter(string[] args)
        {
            if(args.Length == 0)
            {
                Console.WriteLine("Use 'ResourceEditer -h'");
                return false;
            }

            for(int i = 0; i < args.Length; i++)
            {
                string arg = args[i];

                switch (arg.ToLower())
                {
                    case "-h":
                    case "--help":
                        Console.WriteLine("Use 'ResourceEditer -h'");
                        return false;
                    case "-f":
                    case "--file":
                        filename = args[i + 1];

                        if(!File.Exists(filename))
                        {
                            Console.WriteLine($"파일이 존재하지 않습니다.[{filename }]");
                            return false;
                        }

                        Console.WriteLine($"파일명: {filename}");
                        return true;
                    default:
                        Console.WriteLine("잘못된 명령어 입니다.");
                        return false;
                }
            }

            return false;
        }
    }
}
