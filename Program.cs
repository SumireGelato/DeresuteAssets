using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.SQLite;
using System.Diagnostics;
using System.IO;

namespace ss_getnewdata
{
    class Program
    {
        static void Main(string[] args)
        {
            Clear();
            string current_res_ver = "";
            string previous_res_ver = "";
            bool isDiff = true;
            try
            {
                current_res_ver = args[Array.IndexOf(args, "-current") + 1];
                if (current_res_ver == args[0])
                {
                    Environment.Exit(1);
                }
            }
            catch
            {
                Environment.Exit(1);
            }
            try
            {
                previous_res_ver = args[Array.IndexOf(args, "-previous") + 1];
                if (previous_res_ver == args[0])
                {
                    isDiff = false;
                }
            }
            catch
            {
                isDiff = false;
            }
            string current_manifest = "http://storage.game.starlight-stage.jp/dl/" + current_res_ver + "/manifests/Android_AHigh_SHigh";
            string previous_manifest = "";
            if (isDiff)
            {
                previous_manifest = "http://storage.game.starlight-stage.jp/dl/" + previous_res_ver + "/manifests/Android_AHigh_SHigh";
            }
            Process p = new Process();
            p.StartInfo.FileName = "wget.exe";
            p.StartInfo.Arguments = current_manifest;
            p.Start();
            p.WaitForExit();
            File.Move("Android_AHigh_SHigh", "current_manifest");
            if (isDiff)
            {
                p.StartInfo.Arguments = previous_manifest;
                p.Start();
                p.WaitForExit();
                File.Move("Android_AHigh_SHigh", "previous_manifest");
            }
            p.StartInfo.FileName = "lz4er-win.exe";
            p.StartInfo.Arguments = "current_manifest";
            p.Start();
            p.WaitForExit();
            if (isDiff)
            {
                p.StartInfo.Arguments = "previous_manifest";
                p.Start();
                p.WaitForExit();
            }
            SQLiteConnection connection = null;
            if (isDiff)
            {
                connection = new SQLiteConnection("Data Source=previous_manifest.extracted;Version=3;");
                connection.Open();
            }
            //string query = "select hash from manifests where name like \"chara%\" and name like \"%base%\" or name like \"chara%\" and name like \"%icon%\" or name like \"card%\" and name like \"%petit%\"" +
            //"or name like \"card%\" and name like \"%live%\" or name like \"card%\" and name like \"%sign%\" or name like \"card%\" and name like \"%xl%\" or name like \"card%\" and name like \"%bg%\"" + 
            //"or name like \"card%\" and name like \"%m%\"or name like \"comic%\" and name like \"%m.%\" or name like \"jacket%\" or name like \"bg_2%\"";
            string query = "select hash from manifests where name like \"room%\"";
            SQLiteCommand command = null;
            SQLiteDataReader reader = null;
            if (isDiff)
            {
                command = new SQLiteCommand(query, connection);
                reader = command.ExecuteReader();
            }
            Stack<string> stack1 = new Stack<string>();
            if (isDiff)
            {
                while (reader.Read())
                {
                    stack1.Push(reader["hash"].ToString());
                }
                connection.Close();
            }
            connection = new SQLiteConnection("Data Source=current_manifest.extracted;Version=3;");
            connection.Open();
            command = new SQLiteCommand(query, connection);
            reader = command.ExecuteReader();
            string text1 = "";
            while (reader.Read())
            {
                if (!stack1.Contains(reader["hash"].ToString()))
                {
                    text1 = text1 + "http://storage.game.starlight-stage.jp/dl/resources/High/AssetBundles/Android/" + reader["hash"].ToString() + "\r\n";
                }
            }
            connection.Close();
            File.WriteAllText(current_res_ver + ".txt", text1);
            p.StartInfo.FileName = "wget.exe";
            p.StartInfo.Arguments = "-i " + current_res_ver + ".txt -P tmp/";
            p.Start();
            p.WaitForExit();
            File.Delete(current_res_ver + ".txt");
            DirectoryInfo d = new DirectoryInfo(".\\");
            p.StartInfo.FileName = "unitystudio\\Unity Studio.exe";
            DirectoryInfo tmp = new DirectoryInfo(".\\tmp");
            foreach (FileInfo f in tmp.EnumerateFiles("*"))
            {
                File.Move(f.FullName, f.FullName + ".unity3d.lz4");
                p.StartInfo.Arguments = "-assetbundle " + f.FullName +".unity3d.lz4";
                p.Start();
                p.WaitForExit();
            }
            Clear();
        }

        static void Clear()
        {
            try
            {
                DirectoryInfo d = new DirectoryInfo(".\\tmp");
                foreach (FileInfo f in d.EnumerateFiles("*"))
                {
                    File.Delete(f.FullName);
                }
            }
            catch { }
            try
            {
                File.Delete("Android_AHigh_SHigh");
            }
            catch { }
            try
            {
                File.Delete("current_manifest");
            }
            catch { }
            try
            {
                File.Delete("previous_manifest");
            }
            catch { }
            try
            {
                File.Delete("current_manifest.extracted");
            }
            catch { }
            try
            {
                File.Delete("previous_manifest.extracted");
            }
            catch { }
        }
    }
}