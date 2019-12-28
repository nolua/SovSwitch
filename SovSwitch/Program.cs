using System;
using System.Configuration;
using System.Collections;
using System.Collections.Specialized;
using LogLibrary;
using System.IO;
using CheckLibrary;
using CryptoLibrary;
using ConfigLibrary;

namespace SovSwitch
{
    class Program
    {
        static NameValueCollection conf = new NameValueCollection();
        //static string sel;

        //public static string Sel { get => sel; set => sel = value; }

        static void Main(string[] args)
        {
            GetConf();
            String pidFileName = conf["PathFileLog"] + "/pidfile";
            bool backupStatus = true;
            //sel = "louna";

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
                LogToFile.LogAppend(conf["PathFileLog"], conf["FileLogTemp"], $"{new String('+', 10)} debut de la sauvegarde le " +
                    $"{DateTime.Now.ToLongDateString()} à {DateTime.Now.ToLongTimeString()} ");

                // pour test
                File.Delete(pidFileName);
                // pidFile exist => une sauvegarde est en cours ou il y a une erreur?
                if (File.Exists(@pidFileName))
                {
                    Exit(conf["PathFileLog"], conf["FileLogTemp"], "Une sauvegarde est deja en cours, ou il y a une erreur");
                    backupStatus = false;
                }
                // pidFile n'existe pas, on le créé et on continu
                else
                {
                    using (StreamWriter pidFile = File.CreateText(pidFileName))
                    {
                        pidFile.Close();
                    }
                    Uri uri = new Uri("ftp://" + conf["FtpAdresseIp"]);
                    // validation du serveur ftp, on sauvegarde les switchs
                    if (Check.CheckFtpServeur(uri, conf["FtpUser"], conf["FtpPassword"]))
                    {
                        // validation des mots de passe passwordSwitch et passwordEn
                        string passwordSwitch = Aes.DecryptString(conf["PasswordSwitch"], Sel.Val);
                        string passwordEn = Aes.DecryptString(conf["PasswordEn"], Sel.Val);
                        // si le retour de la fonction encrypt est une chaine vide, on sort
                        if (passwordSwitch.Equals("") || passwordEn.Equals(""))
                        {
                            Exit(conf["pathFileLog"], conf["FileLogTemp"],"ERREUR : mot de passe incorect");
                            if (backupStatus)
                                backupStatus = false;
                        }
                        // sinon on continu
                        else
                        {
                            // affecte la section des switchs
                            ListeSwitchSection sectionSwitch = (ListeSwitchSection)ConfigurationManager.GetSection("ListeSwitchsSection");

                            // log du demarrage de la sauvegarde
                            //LogToFile.LogAppend(conf["PathFileLog"], conf["FileLogTemp"], $"{new String('*', 10)} debut de la sauvegarde le {DateTime.Now.ToLongDateString()} à {DateTime.Now.ToLongTimeString()} ");
                            foreach (Switch switchElement in sectionSwitch.Listes)
                            {
                                // on recupere le nom du switch et son adresse IP
                                string SwitchName = switchElement.SwitchName;
                                string SwitchIp = switchElement.SwitchIp;
                                // validation de l'adresse du switch
                                if (!Check.VerifAdresseIp(SwitchIp))
                                {
                                    // si on valide on sort
                                    Exit(conf["PathFileLog"], conf["FileLogTemp"], "Switch " + SwitchName + "(" + SwitchIp + ")" + " : Erreur adresse Ip");
                                    if (backupStatus)
                                        backupStatus = false;
                                }
                                // test ping switch ne repond pas
                                else if (!Check.PingIp(SwitchIp))
                                {
                                    // ping non  valide on sort
                                    Exit(conf["PathFileLog"], conf["FileLogTemp"], "Switch " + SwitchName + "(" + SwitchIp + ")" + " : ne repond pas au ping");
                                    if (backupStatus)
                                        backupStatus = false;
                                }
                                // données validées, on envoi toutes les info sur l'instance cisco
                                else
                                {
                                    Cisco cisco = new Cisco(conf, SwitchName, SwitchIp, Sel.Val);
                                    
                                    Console.WriteLine("etat du backup de sauvegarde du switch => " + cisco.BackupState);
                                    if (backupStatus)
                                        backupStatus = cisco.BackupState;
                                }
                            }
                        }
                    }
                    // serveur ftp non valide
                    else
                    {
                        Exit(conf["PathFileLog"], conf["FileLogTemp"], "Erreur serveur Ftp : " + conf["FtpAdresseIp"]);
                        if (backupStatus)
                            backupStatus = false;
                    }
                }

                // recuperation de la liste des @ mails
                Hashtable sectionListeMail = (Hashtable)ConfigurationManager.GetSection("ListeMail");
                // envoi du mail de log
                Console.WriteLine("Etat du backup general => " + backupStatus);
                //SendMail sendMail = new SendMail(sectionListeMail, conf["PathFileLog"], conf["FileLogTemp"],conf["SmtpServeur"], conf["SenderFrom"],backupStatus);

                //stop timestamp
                LogToFile.LogAppend(conf["PathFileLog"], conf["FileLogTemp"], $"{new String('+', 10)} " +
                   $"fin de la sauvegarde le {DateTime.Now.ToLongDateString()} à {DateTime.Now.ToLongTimeString()} ");
                //LogToFile.LogAppend(conf["PathFileLog"], conf["FileLogTemp"], "\n\r");

                // copie du log temporaire dans le log final
                LogToFile.AppendShortToFinalLog(conf["PathFileLog"], conf["fileLogTemp"], conf["FileLogFinal"]);

                if (backupStatus)
                    File.Delete(pidFileName);
 }
            Console.WriteLine("press a key to exit");
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
            //LogToFile.LogAppend(pathFileLog, fileLog, msgLog + (char)13);
            LogToFile.LogAppend(pathFileLog, fileLog, msgLog);
        }
    }
}

