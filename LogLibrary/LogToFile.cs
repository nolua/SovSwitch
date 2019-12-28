using System;
using System.IO;
using System.IO.Compression;
using System.Text.RegularExpressions;

namespace LogLibrary
{
    public class LogToFile
    {

        public static void Log(string pathFileLog, string fileLog, string logMessage)
        {
            string target = pathFileLog + "/" + fileLog;
            StreamWriter streamWriter = File.CreateText(@target);
            using (streamWriter)
            {
                //w.Write("\r\nLog Entry : ");
                //w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                //w.WriteLine("  :");
                streamWriter.WriteLine(logMessage);
                //w.WriteLine("-------------------------------");
            }
            streamWriter.Close();
        }

        public static void LogAppend(string pathFileLog, string fileLog, string logMessage)
        {
            string target = pathFileLog + "/" + fileLog;
            StreamWriter streamWriter = File.AppendText(@target);
            using (streamWriter)
            {
                //w.Write("\r\nLog Entry : ");
                //w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
                //w.WriteLine("  :");
                streamWriter.WriteLine(logMessage);
                //w.WriteLine("-------------------------------");
            }
            streamWriter.Close();
        }

        //public static void DumpLog(StreamReader r)
        //{
        //    string pathFileLog = "c:/temp/SovSswitch/cisco01.txt";
        //    using (StreamReader r = File.OpenText(@pathFileLog))
        //    string line;
        //    while ((line = r.ReadLine()) != null)
        //    {
        //        Console.WriteLine(line);
        //    }
        //}

        public static void AppendShortToFinalLog(string pathFileLog, string fileLogTemp, string fileLogFinal)
        {
            String line;
            string text = "";
            string source = pathFileLog + "/" + fileLogTemp;
            //string target = pathFileLog + "/" + fileLogFinal;
            try
            {
                //Pass the file path and file name to the StreamReader constructor
                StreamReader sr = new StreamReader(@source);
                //Read the first line of text
                line = sr.ReadLine();
                //Continue to read until you reach end of file
                while (line != null)
                {
                    text += line + "\r\n";
                    //Read the next line
                    line = sr.ReadLine();
                }
                //close the files
                sr.Close();
                //Console.ReadLine();
                LogAppend(pathFileLog, fileLogFinal, text);
                //Console.ReadLine();
                //File.Delete(source);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
        }

        public static void ArchiveLog(string pathFileLog, string fileLogFinal)
        {
            string date = DateTime.Now.ToString("ddMMyyyy");
            string heure = DateTime.Now.ToString("HHmmss");
            // on extrait l'extension .log
            string[] elements = Regex.Split(fileLogFinal, ".log");
            // on merge le pathFileLog avec le fichier fileLogFinal
            string pathFileToZip = pathFileLog + '/' + fileLogFinal;
            // on créé le zipfile avec le nom du fichier log, la date et l'heure
            string zipFile = elements[0] + '_' + date + '-' + heure + ".zip";
            // on merge le pathFileLog avec le zipFile
            string pathFileZip = pathFileLog + '/' + zipFile;


            using (FileStream fs = new FileStream(@pathFileZip, FileMode.Create))
            using (ZipArchive arch = new ZipArchive(fs, ZipArchiveMode.Create))
            {
                arch.CreateEntryFromFile(@pathFileToZip, fileLogFinal);
                File.Delete(@pathFileToZip);
            }

        }
    }
}
