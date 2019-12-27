using ConfigLibrary;
using System;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;
using LogLibrary;
using System.IO;
using CheckLibrary;
using CryptoLibrary;
using System.Threading;

namespace SovSwitch
{
    class Program
    {
        static NameValueCollection conf = new NameValueCollection();

        static void Main(string[] args)
        {
            GetConf();
            String pidFileName = conf["PathFileLog"] + "/pidfile";
            string sel = "louna";

            if (args.Length != 0)
            {
                if (args[0].Equals("/encrypt") && !args[1].Equals(""))
                {
                    string strResult = Aes.EncryptString(args[1], "louna");
                    Console.WriteLine(strResult);
                }
                else
                {
                    ExitCmd(4);
                }
            }
            else
            {
                // logtemp exist => delete and create
                string tempFile = conf["PathFileLog"] + "/" + conf["FileLogTemp"];
                if (File.Exists(@tempFile))
                {
                    File.Delete(@tempFile);
                }
                // start timestamp
                LogToFile.LogAppend(conf["PathFileLog"], conf["FileLogTemp"], $"{new String('*', 10)} debut de la sauvegarde le " +
                    $"{DateTime.Now.ToLongDateString()} à {DateTime.Now.ToLongTimeString()} ");

                // pidFile exist => une sauvegarde est en cours ou il y a une erreur?
                if (File.Exists(@pidFileName))
                {
                    Exit(conf["PathFileLog"], conf["FileLogTemp"], "Une sauvegarde est deja en cours, ou il y a une erreur : ");
                }
                // pidFile n'existe pas, on le créé et on continu
                else
                {
                    using (StreamWriter pidFile = File.CreateText(pidFileName))
                    {
                        pidFile.Close();
                    }

                }



                //stop timestamp
                LogToFile.LogAppend(conf["PathFileLog"], conf["FileLogTemp"], $"{new String('*', 10)} " +
                   $"fin de la sauvegarde le {DateTime.Now.ToLongDateString()} à {DateTime.Now.ToLongTimeString()} ");
                LogToFile.LogAppend(conf["PathFileLog"], conf["FileLogTemp"], "\n\r");
            }
            Console.WriteLine("\n press a key to exit");
            Console.ReadKey();
        }


        // creation du dictionnaire des parmatres
        private static void GetConf()
        {
            // recuperation du path du fichier log dans appSetting de l'adresse smtp et du sender 
            conf["PathFileLog"] = ConfigurationManager.AppSettings["PathFileLog"];
            conf["FileLogTemp"] = ConfigurationManager.AppSettings["FileLogTemp"];
            conf["FileLogFinal"] = ConfigurationManager.AppSettings["FileLogFinal"];
            conf["SmtpServeur"] = ConfigurationManager.AppSettings["SmtpServeur"];
            conf["SenderFrom"] = ConfigurationManager.AppSettings["SenderFrom"];

            // recupere le setting du serveur ftp
            Hashtable sectionFtpSetting = (Hashtable)ConfigurationManager.GetSection("FtpSetting");
            conf["FtpAdresseIp"] = (string)sectionFtpSetting["FtpAdresseIp"];
            conf["FtpUser"] = (string)sectionFtpSetting["FtpUser"];
            conf["FtpPassword"] = (string)sectionFtpSetting["FtpPassword"];
            conf["FtpSuffix"] = (string)sectionFtpSetting["FtpSuffix"];

            // affecte la section des mot de passe des switchs
            Hashtable sectionPassword = (Hashtable)ConfigurationManager.GetSection("PasswordSwitch");
            conf["Username"] = (string)sectionPassword["Username"];
            conf["PasswordSwitch"] = (string)sectionPassword["PasswordSwitch"];
            conf["PasswordEn"] = (string)sectionPassword["PasswordEn"];
        }

        private static void ExitCmd(int v)
        {
            switch (v)
            {
                case 1:
                    Console.WriteLine("Argument de ligne de commande incorrect");
                    break;

                default:
                    Console.WriteLine("Default case");
                    break;
            }
        }

        public static void Exit(string pathFileLog, string fileLog, string msgLog)
        {
            Console.WriteLine();
            Console.WriteLine(msgLog);
            LogToFile.LogAppend(pathFileLog, fileLog, msgLog + (char)13);
        }
    }
}

