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
            GetConf();
            String pidFileName = conf["PathFileLog"]+ "/pidfile";
            
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
                // si le fichier existe, on a une sauvegarde deja en cours ou il y a eu un 
                // pb avec la precedente sauvegarde => on sort
                if (File.Exists(@pidFileName))
                {
                    exit(conf["PathFileLog"], conf["FileLogFinal"], 5, $" Une sauvegarde est deja en cours, ou il y a une erreur : " +
                        $"{DateTime.Now.ToLongDateString()} à {DateTime.Now.ToLongTimeString()}");
                }
                // sinon on le crée et on rentre dans le programme
                else
                {
                    

                    using (StreamWriter pidFile = File.CreateText(pidFileName))
                    {
                        //sw.WriteLine("Hello");
                        //sw.WriteLine("And");
                        //sw.WriteLine("Welcome");
                        pidFile.Close();
                    }
                    

                    string SwitchName;
                    string SwitchIp;

                    // on efface le fichier de log temporaire si present
                    string tempFile = conf["PathFileLog"] + "\\" + conf["FileLogTemp"];
                    if (File.Exists(@tempFile))
                    {
                        File.Delete(@tempFile);
                    }

                    // on valide le serveur ftp
                    Uri uri = new Uri("ftp://" + conf["FtpAdresseIp"]);

                    // validation du serveur ftp, on sauvegarde les switchs
                    if (Check.CheckFtpServeur(uri, conf["FtpUser"], conf["FtpPassword"]))
                    {


                        // affecte la section des switchs
                        ListeSwitchSection sectionSwitch = (ListeSwitchSection)ConfigurationManager.GetSection("ListeSwitchsSection");

                        // log du demarrage de la sauvegarde
                        LogToFile.LogAppend(conf["PathFileLog"], conf["FileLogTemp"], $"{new String('*', 10)} debut de la sauvegarde le {DateTime.Now.ToLongDateString()} à {DateTime.Now.ToLongTimeString()} ");
                        foreach (Switch switchElement in sectionSwitch.Listes)
                        {
                            // on recupere le nom du switch et son adresse IP
                            SwitchName = switchElement.SwitchName;
                            SwitchIp = switchElement.SwitchIp;
                            // validation de l'adresse du switch
                            if (!Check.VerifAdresseIp(SwitchIp))
                            {
                                // si on valide on sort
                                exit(conf["PathFileLog"], conf["FileLogTemp"], 2, SwitchName + "(" + SwitchIp + ")" + " : Erreur adresse Ip");
                            }
                            // test ping switch ne repond pas
                            else if (!Check.PingIp(SwitchIp))
                            {
                                // ping non  valide on sort
                                exit(conf["PathFileLog"], conf["FileLogTemp"], 3, "Switch " + SwitchName + "(" + SwitchIp + ")" + " injoignable");
                            }
                            // données validées, on envoi toutes les info sur l'instance cisco
                            else
                            {
                                Cisco cisco = new Cisco(conf, SwitchName, SwitchIp);
                            }
                        }
                        LogToFile.LogAppend(conf["PathFileLog"], conf["FileLogTemp"], $"{new String('*', 10)} fin de la sauvegarde le {DateTime.Now.ToLongDateString()} à {DateTime.Now.ToLongTimeString()} ");
                        LogToFile.LogAppend(conf["PathFileLog"], conf["FileLogTemp"], "\n\r");
                        File.Delete(pidFileName);
                    }
                    // serveur ftp non valide, on sort
                    else
                    {
                        exit(conf["PathFileLog"], conf["FileLogTemp"], 1, "Erreur serveur Ftp : " + conf["FtpAdresseIp"]);
                    }

                    // recuperation de la liste des @ mails
                    Hashtable sectionListeMail = (Hashtable)ConfigurationManager.GetSection("ListeMail");

                    // envoi du mail de log
                    //SendMail sendMail = new SendMail(sectionListeMail, conf["PathFileLog"], conf["FileLogTemp"],conf["SmtpServeur"], conf["SenderFrom"]);

                    // copie du log temporaire dans le log final
                    LogToFile.AppendShortToFinalLog(conf["PathFileLog"], conf["fileLogTemp"], conf["FileLogFinal"]);

                    //// affichage du fichier log
                    //using (StreamReader r = File.OpenText(@pathFileLog))
                    //{
                    //    LogToFile.DumpLog(r);
                    //}
                }
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

        private static void exit(string pathFileLog,string fileLog,int v,string msgLog)
        {
            //throw new NotImplementedException();
            
            switch (v)
            {
                case 1: //erreur serveur ftp
                    Console.WriteLine();
                    Console.WriteLine(msgLog);
                    LogToFile.LogAppend(pathFileLog,fileLog,msgLog+(char)13);
                    break;
                case 2:// adresse Ip invalide
                    Console.WriteLine();
                    Console.WriteLine(msgLog);
                    LogToFile.LogAppend(pathFileLog,fileLog, msgLog + (char)13);
                    break;
                case 3://ping nok
                    Console.WriteLine();
                    Console.WriteLine(msgLog);
                    LogToFile.LogAppend(pathFileLog,fileLog, msgLog + (char)13);
                    break;
                case 5://sauvegarde deja en cours
                    Console.WriteLine();
                    Console.WriteLine(msgLog);
                    LogToFile.LogAppend(pathFileLog, fileLog, msgLog + (char)13);
                    break;
                default:
                    Console.WriteLine("Default case");
                    break;
            }
        }
    }
}
