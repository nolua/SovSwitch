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
        static NameValueCollection conf  = new NameValueCollection();

        static void Main(string[] args)
        {

            //Cisco cisco = null;
            //Uri uri = null;
            //SendMail sendMail = null;
            GetConf();

            if (args.Length != 0)
            {
                if (args[0].Equals("/encrypt") && !args[1].Equals(""))
                {
                    string strResult = Aes.EncryptString(args[1], "louna");
                    Console.WriteLine(strResult);
                }
                else
                {
                    exit(4);
                }
            }
            else
            {
                string SwitchName = "";
                string SwitchIp = "";
                
                

                // on efface le fichier de log temporaire si present
                string tempFile = conf["PathFileLog"] + "\\" + conf["FileLogTemp"];
                if (File.Exists(@tempFile))
                {
                    File.Delete(@tempFile);
                }


                // on valide le serveur ftp
                Uri uri = new Uri("ftp://" + conf["FtpAdresseIp"]);

                // validation du serveur ftp
                if (Check.CheckFtpServeur(uri, conf["FtpUser"], conf["FtpPassword"]))
                {

                    
                    // affecte la section des switchs
                    ListeSwitchSection sectionSwitch = (ListeSwitchSection)ConfigurationManager.GetSection("ListeSwitchsSection");

                    // log du demarrage de la sauvegarde
                    LogToFile.Log(conf["PathFileLog"], conf["FileLogTemp"],$"{new String('*', 10)} debut de la sauvegarde le {DateTime.Now.ToLongDateString()} à {DateTime.Now.ToLongTimeString()} ");
                    foreach (Switch switchElement in sectionSwitch.Listes)
                    {
                        // on recupere le nom du switch et son adresse IP
                        SwitchName = switchElement.SwitchName;
                        SwitchIp = switchElement.SwitchIp;
                        // validation de l'adresse du switch
                        if (!Check.VerifAdresseIp(SwitchIp))
                        {
                            // si on valide on sort
                            exit(conf["PathFileLog"],conf["FileLogTemp"], 2, SwitchName + "(" + SwitchIp + ")" + " : Erreur adresse Ip");
                        }
                        // test ping switch
                        else if (!Check.PingIp(SwitchIp))
                        {
                            // si on valide on sort
                            exit(conf["PathFileLog"], conf["FileLogTemp"], 3, "Switch " + SwitchName + "(" + SwitchIp + ")" + " injoignable");
                        }
                        // données validées, on envoi toutes les info sur l'instance cisco
                        else
                        {
                            //Cisco cisco = new Cisco(conf, SwitchIp, adrSwitch);
                        }
                    }
                    LogToFile.Log(conf["PathFileLog"], conf["FileLogTemp"],$"{new String('*', 10)} fin de la sauvegarde le {DateTime.Now.ToLongDateString()} à {DateTime.Now.ToLongTimeString()} ");
                    LogToFile.Log(conf["PathFileLog"], conf["FileLogTemp"], "\n\r");
                }
                else
                {
                    exit(conf["PathFileLog"], conf["FileLogTemp"], 1, "Erreur serveur Ftp : " + conf["FtpAdresseIp"]);
                }
                // recuperation des @ mails
                

                Hashtable sectionListeMail = (Hashtable)ConfigurationManager.GetSection("ListeMail");


                // envoi du mail de log
                //SendMail sendMail = new SendMail(sectionListeMail, conf["PathFileLog"], conf["FileLogTemp"],conf["SmtpServeur"], conf["SenderFrom"]);

                
                // copie du log temporaire dans le log final
                LogToFile.AppendShortToFinalLog(conf["PathFileLog"], conf["fileLogTemp"],conf["FileLogFinal"]);

                //// affichage du fichier log
                //using (StreamReader r = File.OpenText(@pathFileLog))
                //{
                //    LogToFile.DumpLog(r);
                //}

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

            // affecte la section FtpSetting
            Hashtable sectionFtpSetting = (Hashtable)ConfigurationManager.GetSection("FtpSetting");

            // recupere l'adresse ip du serveur ftp
            conf["FtpAdresseIp"] = (string)sectionFtpSetting["FtpAdresseIp"];
            //Console.WriteLine("\tFtpIp => {0}", sectionFtpSetting["FtpAdresseIp"]);

            

            // affecte les credentials du serveur ftp
            conf["FtpUser"] = (string)sectionFtpSetting["FtpUser"];
            conf["FtpPassword"] = (string)sectionFtpSetting["FtpPassword"];

            // recupere le suffixe ftp
            conf["FtpSuffix"] = (string)sectionFtpSetting["FtpSuffix"];

            // affecte la section des mot de passe des switchs
            Hashtable sectionPassword = (Hashtable)ConfigurationManager.GetSection("PasswordSwitch");

            // on affecte le passwordSwitch
            conf["PasswordSwitch"] = (string)sectionPassword["PasswordSwitch"];
            //Console.WriteLine("\tPasswordEn => {0}", sectionPassword["PasswordEn"]);

            // on affecte le password EN du switch
            conf["PasswordEn"] = (string)sectionPassword["PasswordEn"];

            


        }

        private static void exit(int v)
        {
            switch (v)
            {
                case 4:
                    Console.WriteLine("Argument de ligne de commande incorrect");
                    break;
                
                default:
                    Console.WriteLine("Default case");
                    break;
            }
        }

        private static void exit(string pathFileLog,string fileLogTemp,int v,string msgLog)
        {
            //throw new NotImplementedException();
            
            switch (v)
            {
                case 1:
                    Console.WriteLine(msgLog);
                    LogToFile.Log(pathFileLog,fileLogTemp,msgLog);
                    break;
                case 2:
                    Console.WriteLine(msgLog);
                    LogToFile.Log(pathFileLog,fileLogTemp, msgLog);
                    break;
                case 3:
                    Console.WriteLine(msgLog);
                    LogToFile.Log(pathFileLog,fileLogTemp, msgLog);
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }
        }
    }
}
