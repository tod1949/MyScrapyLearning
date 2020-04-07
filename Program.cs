using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

//添加正则表达式，添加表格，添加IO
using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using System.Runtime.InteropServices;

namespace MyScrapyLearning
{
    class Program
    {
        static void Main(string[] args)
        {
            //Html Download
            byte[] thebuffer = null;
            //https://music.douban.com/chart
            System.Net.WebClient webClient = new System.Net.WebClient();
            thebuffer = webClient.DownloadData("https://music.douban.com/chart");
            string html = System.Text.Encoding.GetEncoding("utf-8").GetString(thebuffer);

            //utf-8 gb2312 gbk uft-16

            //Analisys
            //通过正则表达式获取内容
            Regex r = new Regex("");
            MatchCollection matches = new Regex("<a href=\"javascript:;\">([\\s\\S]*?)</a>").Matches(html);
            int count = matches.Count;
            string[] input = new string[count];
            for (int i = 0; i < count; i++)
            {
                input[i] = matches[i].Result("$1");
                Console.WriteLine(input[i]);
            }

            //Export
            Console.ReadLine();
            DataTable dt = new DataTable();
            DataColumnCollection col = dt.Columns;
            col.Add("Tag of Song",typeof(string));
            for (int i = 0; i < count; i++)
            {
                DataRow row = dt.NewRow();
                row["Tag of Song"] = input[i];
                dt.Rows.Add(row);
            }
            string filename = "output.xls";
            FileStream myfile = new FileStream(filename,System.IO.FileMode.Create, System.IO.FileAccess.Write);
            StreamWriter sw = new StreamWriter(myfile, System.Text.Encoding.Default);
            //Starting to export data
            string data = "";
            for (int i = 0; i < col.Count; i++)
            {
                data += dt.Columns[i].ToString();
                if (i < col.Count - 1)
                    data += ",";
            }
            sw.WriteLine(data);
            for (int t = 0; t < dt.Rows.Count; t++)
            {
                data = "";
                for (int i = 0; i < col.Count; i++)
                {
                    data += dt.Rows[t][i].ToString();
                    if (i < col.Count - 1)
                        data += ",";
                }
                sw.WriteLine(data);
            }
            sw.Close();
            myfile.Close();
        }
    }
}
