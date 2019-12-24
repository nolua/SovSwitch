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
        static void Main(string[] args)
        {

            //Cisco cisco = null;
            //Uri uri = null;
            //SendMail sendMail = null;
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
                string nameSwitch = "";
                string adrSwitch = "";
                NameValueCollection conf = new NameValueCollection();

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
                // on valide le serveur ftp
                Uri uri = new Uri("ftp://" + conf["FtpAdresseIp"]);

                // affecte les credentials du serveur ftp
                conf["FtpUser"] = (string)sectionFtpSetting["FtpUser"];
                conf["FtpPassword"] = (string)sectionFtpSetting["FtpPassword"];

                // validation du serveur ftp
                if (Check.CheckFtpServeur(uri, conf["FtpUser"], conf["FtpPassword"]))
                {
                    // recupere le suffixe ftp
                    conf["FtpSuffix"] = (string)sectionFtpSetting["FtpSuffix"];
                    //Console.WriteLine("\tftp Suffix => {0}", sectionFtpSetting["FtpSuffix"]);

                    // affecte la section des mot de passe des switchs
                    Hashtable sectionPassword = (Hashtable)ConfigurationManager.GetSection("PasswordSwitch");

                    // on affecte le passwordSwitch
                    conf["PasswordSwitch"] = (string)sectionPassword["PasswordSwitch"];
                    //Console.WriteLine("\tPasswordEn => {0}", sectionPassword["PasswordEn"]);

                    // on affecte le password EN du switch
                    conf["PasswordEn"] = (string)sectionPassword["PasswordEn"];

                    // affecte la section des switchs
                    ListeSwitchSection sectionSwitch = (ListeSwitchSection)ConfigurationManager.GetSection("ListeSwitchsSection");
                    // log du demarrage de la sauvegarde
                    LogToFile.Log(conf["PathFileLog"], conf["FileLogTemp"],$"{new String('*', 10)} debut de la sauvegarde le {DateTime.Now.ToLongDateString()} à {DateTime.Now.ToLongTimeString()} ");
                    foreach (Switch switchElement in sectionSwitch.Listes)
                    {
                        // on recupere le nom du switch et son adresse IP
                        nameSwitch = switchElement.SwitchName;
                        adrSwitch = switchElement.SwitchIp;
                        // validation de l'adresse du switch
                        if (!Check.VerifAdresseIp(adrSwitch))
                        {
                            // si on valide on sort
                            exit(conf["PathFileLog"],conf["FileLogTemp"], 2, nameSwitch + "(" + adrSwitch + ")" + " : Erreur adresse Ip");
                        }
                        // test ping switch
                        else if (!Check.PingIp(adrSwitch))
                        {
                            // si on valide on sort
                            exit(conf["PathFileLog"], conf["FileLogTemp"], 3, "Switch " + nameSwitch + "(" + adrSwitch + ")" + " injoignable");
                        }
                        // données validées, on envoi toutes les info sur l'instance cisco
                        else
                        {
                            //Cisco cisco = new Cisco(conf, nameSwitch, adrSwitch);
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

                // tempo d'envoi du mail
                //Thread.Sleep(300000);

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
