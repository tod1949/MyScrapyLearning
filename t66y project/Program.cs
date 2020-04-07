using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using System.Text.RegularExpressions;
using System.Data;
using System.Net;
using System.Net.Http.Headers;

namespace t66y_project
{
    public class NewWebClient : WebClient
    {
        private int _timeout;

        /// <summary>
        /// 超时时间(毫秒)
        /// </summary>
        public int Timeout
        {
            get
            {
                return _timeout;
            }
            set
            {
                _timeout = value;
            }
        }

        public NewWebClient()
        {
            this._timeout = 60000;
        }

        public NewWebClient(int timeout)
        {
            this._timeout = timeout;
        }

        protected override WebRequest GetWebRequest(Uri address)
        {
            var result = base.GetWebRequest(address);
            result.Timeout = this._timeout;
            return result;
        }
    }

    class Program
    {
        public static class DownLoad_HTML
        {
            private static int FailCount = 0; //记录下载失败的次数

            public static string GetHtml(string url) //传入要下载的网址
            {
                string str = string.Empty;
                try
                {
                    System.Net.WebRequest request = System.Net.WebRequest.Create(url);

                    request.Timeout = 10000; //下载超时时间
                    request.Headers.Add("User-Agent","Mozilla/5.0 (Windows NT 10.0; WOW64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/69.0.3497.81 Safari/537.36 Maxthon/5.3.8.2000");
                    System.Net.WebResponse response = request.GetResponse();
                    System.IO.Stream streamReceive = response.GetResponseStream();
                    Encoding encoding = Encoding.GetEncoding("gb2312");//utf-8 网页文字编码
                    System.IO.StreamReader streamReader = new System.IO.StreamReader(streamReceive, encoding);
                    str = streamReader.ReadToEnd();
                    streamReader.Close();
                }
                catch (Exception ex)
                {
                    FailCount++;

                    if (FailCount > 5)
                    {
                        var result = System.Windows.Forms.MessageBox.Show("已下载失败" + FailCount + "次，是否要继续尝试？" + Environment.NewLine + ex.ToString(), "数据下载异常", System.Windows.Forms.MessageBoxButtons.YesNo, System.Windows.Forms.MessageBoxIcon.Error);
                        if (result == System.Windows.Forms.DialogResult.Yes)
                        {
                            str = GetHtml(url);
                        }
                        else
                        {
                            System.Windows.Forms.MessageBox.Show("下载HTML失败" + Environment.NewLine + ex.ToString(), "下载HTML失败", System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Error);
                            throw ex;
                        }
                    }
                    else
                    {
                        str = GetHtml(url);
                    }
                }

                FailCount = 0; //如果能执行到这一步就表示下载终于成功了
                return str;
            }

            //Analysis

            //Record
        }

        private static void FirstLayer(string link)
        {
            byte[] buffer = null;
            NewWebClient webclient = new NewWebClient(20*1000);
            webclient.Headers.Add("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/70.0.3538.102 Safari/537.36 Edge/18.18363");
            buffer = webclient.DownloadData(link);
            string decodedhtml = Encoding.GetEncoding("gbk").GetString(buffer);
            string serverlink = new Regex("https://[\\S]*?/").Match(link).ToString();

            //Analisys
            //通过正则表达式获取内容
            Regex r = new Regex("");
            MatchCollection matches = new Regex("_blank\" id=\"\">([\\s\\S]*?)</a>").Matches(decodedhtml);
            MatchCollection links = new Regex("<h3><a href=\"([\\s\\S]*?)\" target=\"_blank\" id=\"\">([\\s\\S]*?)</a></h3>").Matches(decodedhtml);
            int count = links.Count;
            string[] input = new string[count];
            for (int i = 0; i < count; i++)
            {
                input[i] = links[i].Result("$2");
                //Console.WriteLine(input[i]);
                //Match wanted = new Regex("(双|(?<!(0-9)(3P|3p)))+").Match(input[i]);
                //Match wanted = new Regex("(双|菊|肛|(?<!(\\d))(3p|3P))+").Match(input[i]);
                Match wanted = new Regex("(菊|肛|奸|(双插)|(SPA|spa)|(屁眼)|(大神)|(全集)|(合集)|(留学)|(老外)|(为国争光)|(康爱福)|(调教)|(三穴)|(三通)|(轮插))+").Match(input[i]);
                if (wanted.Value != "")
                {
                    Console.WriteLine(input[i]);
                    string sublink = serverlink + links[i].Result("$1");
                    SecondLayer(sublink,false,1);
                }
            }
        }
        private static string LocalFileReader(string filePath)
        {
            string text = "";
            FileStream myfile = new FileStream(filePath, System.IO.FileMode.Open, System.IO.FileAccess.Read);
            var fileEncoding = Encoding.GetEncoding("utf-8");
            StreamReader sr = new StreamReader(myfile, fileEncoding, true);
            while (!sr.EndOfStream)
            {
                text+= sr.ReadLine();
                text += "\n";
            }
            
            sr.Close();
            myfile.Close();
            return text;
        }
        private static void LocalFileWriter(string filePath, string content)
        {
            FileStream myfile = new FileStream(filePath, System.IO.FileMode.Create, System.IO.FileAccess.Write);
            var fileEncoding = Encoding.GetEncoding("utf-8");
            StreamWriter sw = new StreamWriter(myfile, fileEncoding);
            sw.WriteLine(content);
            sw.Close();
            myfile.Close();
        }
        private static void SecondLayer(string link, bool debugmode, int retrytimes)
        {
            byte[] buffer = null;
            NewWebClient webclient = new NewWebClient(60*1000);
            string currenthtml = "";
            string title = "";
            string htmlbackup = "";
            if (debugmode)
            {
                string filepath = "D:\\page2.txt";
                currenthtml = LocalFileReader(filepath);
            }
            else
            {
                try
                {
                    buffer = webclient.DownloadData(link);
                    currenthtml = Encoding.GetEncoding("gbk").GetString(buffer);
                }
                catch
                {
                    if (retrytimes < 5)
                        SecondLayer(link, debugmode, retrytimes++);
                    else
                        return;
                }
            }
            //Get the title
            Match titlematch = new Regex("(?<=<meta name=\"description\" content=\")[\\s\\S]*(?= 草榴社區 t66y\\.com\" />)").Match(currenthtml);
            title = titlematch.Value;
            title = title.Replace("/", "_");
            title = title.Replace("[", "_");
            title = title.Replace("]", "_");
            title = title.Replace("\\", "");
            title = title.Replace("?", "");
            title = title.Replace("*", "");

            //Get rid of  all the Quotes
            htmlbackup = currenthtml;
            Match match1 = new Regex("<br><h6 class=\"quote\">Quote:</h6><blockquote>[\\s\\S]*</blockquote><br>").Match(currenthtml);
            if (match1.Value != "")
                currenthtml = currenthtml.Replace(match1.Value, "");

            //Get all the images
            MatchCollection matchjpg = new Regex("(?<=ess-data=')((?!ess-data=')[\\s\\S])*?\\.(jpg|png)(?='>)").Matches(currenthtml);
            //Very interesting ((?!xxx).) is a good trick
            if (matchjpg.Count == 0)
            {
                currenthtml = htmlbackup;
                matchjpg = new Regex("(?<=ess-data=')((?!ess-data=')[\\s\\S])*?\\.(jpg|png)(?='>)").Matches(currenthtml);
            }
            string imgfilename = "";
            string filedictory = "d:\\my t66y\\" + title + "\\";
            if (!Directory.Exists(filedictory))
                Directory.CreateDirectory(filedictory);
            
            for (int i = 0; i < matchjpg.Count; i++)
            {
                match1 = new Regex("(?<=/)((?!/).)+\\.(jpg|png)").Match(matchjpg[i].Value);
                imgfilename = match1.Value;
                try
                {
                    string _path = "d:\\my t66y\\" + title + "\\" + imgfilename;
                    if (!File.Exists(_path))
                        webclient.DownloadFile(new Uri(matchjpg[i].Value), _path);
                }
                catch (WebException wb)
                {
                    if (wb.Status == WebExceptionStatus.Timeout)
                    {

                    }
                    if (i > matchjpg.Count / 2)
                        continue;
                    else
                        if (retrytimes < 5)
                        SecondLayer(link, debugmode, retrytimes++);
                    else
                        return;
                }
            }
            

            //Get download link
            match1 = new Regex("http://www\\.rmdown\\.com/[\\s\\S]*?(?=</a>)").Match(currenthtml);
            string downloadlink = match1.Value;

            if (downloadlink != "")
            {
                string hashlink = new Regex("(?<=http://www\\.rmdown\\.com/link\\.php\\?hash=)[\\s\\S]*").Match(downloadlink).Value;
                /*
                string filename = filedictory + "downloadlink.txt";
                FileStream myfile = new FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
                StreamWriter sw = new StreamWriter(myfile, System.Text.Encoding.Default);
                sw.WriteLine(downloadlink);
                sw.Close();
                myfile.Close();
                */
                //webclient.Headers.Add("GET ",hashlink);
                webclient.Headers.Add("Referer",downloadlink);
                webclient.Headers.Add("Cookie", "__cfduid=de6459d21de4fe09c8cf6754f71bbae1a1584457144");

                string torrentslink = "http://www.rmdown.com/download.php?reff=110&ref="+hashlink;
                WebRequest myrequest = WebRequest.Create(downloadlink);
                myrequest.Timeout = 30 * 1000;
                if (!File.Exists(filedictory + "downloadlink.txt"))
                    LocalFileWriter(filedictory + "downloadlink.txt", downloadlink);
                //myrequest.GetResponse();
                try
                {
                    string _path = "d:\\my t66y\\" + title + "\\" + "torrent.torrent";
                    if (!File.Exists(_path))
                    {
                        string password = myrequest.GetResponse().Headers["Set-Cookie"];
                        /*
                        buffer = webclient.DownloadData(downloadlink);

                        string t = Encoding.GetEncoding("utf-8").GetString(buffer);
                        string password = webclient.ResponseHeaders["Set-Cookie"];
                        */
                        password = password.Replace("; path=/", "");
                        //webclient.Headers["Cookie"]="__cfduid=de6459d21de4fe09c8cf6754f71bbae1a1584457144;"+password+";";
                        password = new Regex("PHPSESSID=[\\s\\S]*").Match(password).Value;
                        //webclient.Headers["Cookie"] = password;
                        webclient.Headers["Cookie"] = "__cfduid=de6459d21de4fe09c8cf6754f71bbae1a1584457144;" + password + ";";
                        try
                        {
                            if (!File.Exists(_path))
                                webclient.DownloadFile(torrentslink, _path);
                        }
                        catch (WebException wb)
                        {
                            return;
                        }
                    }    
                }
                catch (WebException wb)
                {
                    myrequest.Abort();
                    return;
                }
                Console.WriteLine();
            }
            Console.WriteLine();
            //Get the download link
        }

        private static void ResultOutput(DataTable dt, string _filename)
        {
            //Export
            Console.ReadLine();
            int count = dt.Rows.Count;
            DataColumnCollection col = dt.Columns;
            /*
            col.Add("Tag of Song", typeof(string));
            for (int i = 0; i < count; i++)
            {
                DataRow row = dt.NewRow();
                row["Tag of Song"] = input[i];
                dt.Rows.Add(row);
            }
            string filename = "output.xls";
            FileStream myfile = new FileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write);
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
            */
        }
            
        static void Main(string[] args)
        {
            //Download
            System.Net.ServicePointManager.DefaultConnectionLimit = 10;
            for (int i = 1; i < 20; i++)
            {
                string downloadpage = "https://cl.330f.tk/thread0806.php?fid=25&search=&page=" + i.ToString();
                FirstLayer(downloadpage);
            }
            string pagelink = "https://cl.330f.tk/thread0806.php?fid=25";
            // string downloadlink = "https://music.douban.com/chart";

            //SecondLayer(pagelink, true,1);
            Console.WriteLine("Finish!");
        }
    }
}
