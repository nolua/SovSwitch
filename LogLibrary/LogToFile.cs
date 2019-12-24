﻿using System;
using System.IO;

namespace LogLibrary
{
    public class LogToFile
    {
        
        public static void Log(string pathFileLog, string fileLogTemp,string logMessage)
        {
            string target = pathFileLog + "/" + fileLogTemp;
            StreamWriter streamWriter = File.AppendText(@target);
            using (streamWriter) { 
            //w.Write("\r\nLog Entry : ");
            //w.WriteLine($"{DateTime.Now.ToLongTimeString()} {DateTime.Now.ToLongDateString()}");
            //w.WriteLine("  :");
            streamWriter.WriteLine(logMessage);
            //w.WriteLine("-------------------------------");
            }
            streamWriter.Close();
        }

        public static void LogAppend(string pathFileLog, string logMessage)
        {
            StreamWriter streamWriter = File.AppendText(@pathFileLog);
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

        public static void DumpLog(StreamReader r)
        {
            string line;
            while ((line = r.ReadLine()) != null)
            {
                Console.WriteLine(line);
            }
        }

        public static void AppendShortToFinalLog(string pathFileLog, string fileLogTemp,string fileLogFinal) 
        {
            //using StreamReader srFileLog = File.OpenText(@pathFileLog);
            String line;
            string text="";
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
                    //write the lie to console window
                    //Console.WriteLine(line);
                    text += line + "\r\n";
                    //Read the next line
                    line = sr.ReadLine();
                }
                //close the files
                sr.Close();
                //Console.ReadLine();
                Log(pathFileLog,fileLogFinal, text);
                //Console.ReadLine();
                File.Delete(source);
            }
            catch (Exception e)
            {
                Console.WriteLine("Exception: " + e.Message);
            }
            finally
            {
                Console.WriteLine("Executing finally block.");
            }
            //using StreamWriter streamWriter = File.AppendText(@pathFileLog);
        }

    }
}