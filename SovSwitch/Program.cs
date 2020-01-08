//#define PROD
//#undef PROD

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
    /// <summary>
    /// credentiel utilisé pour la version de test :
    ///  ftp : sovswitch/cisco123
    ///  switch:
    ///  fujitsu/1formatic
    ///  en:cenexi
    /// </summary>
    class Program
    {
        static NameValueCollection conf = new NameValueCollection();
        bool backupStatus;

        static void Main(string[] args)
        {
            GetConf();
            String pidFileName = conf["PathFileLog"] + "/pidfile";

            if (args.Length == 0 || args.Length > 2)
            {
                ExitCmd(1);
            }
            else if (args.Length == 1)
            {
                if (args[0].Equals("/backup"))
                {
                    //verif existence du repertoire log
                    if (!Directory.Exists(conf["PathFileLog"]))
                    {
                        Directory.CreateDirectory(conf["PathFileLog"]);
                    }

                    // pidFile exist => une sauvegarde est en cours ou il y a une erreur ?
                    // on log dans un fichier pidfile.log avec un timestamp
                    if (File.Exists(@pidFileName))
                    {
                        LogToFile.WriteLog(conf["PathFileLog"], conf["FileLogPidFile"], $"Une sauvegarde est deja en cours, ou il y a une erreur");
                        LogToFile.WriteLog(conf["PathFileLog"], conf["FileLogPidFile"], "Veuillez Supprimer le fichier " + conf["PathFileLog"] + "/" + conf["FileLogPidFile"]);
                        LogToFile.WriteLog(conf["PathFileLog"], conf["FileLogPidFile"], $"{ DateTime.Now.ToLongDateString()} à { DateTime.Now.ToLongTimeString()} \r\n");
                    }
                    else
                    {
                        BackupSwitch();
                    }
                }
                else
                {
                    ExitCmd(1);
                }

            }
            else
            {
                if (args[0].Equals("/encrypt"))
                {
                    string strResult = Aes.EncryptString(args[1], Sel.Val);
                    Console.WriteLine(strResult);
                }
                else
                {
                    ExitCmd(1);
                }
            }


#if DEBUG
            Console.WriteLine("press a key to exit");
            Console.ReadKey();
#endif
        }

        private static void BackupSwitch()
        {
            bool backupStatus = true;
            String pidFileName = conf["PathFileLog"] + "/pidfile";

            // pidFile n'existe pas, on le créé et on continu
            string fileLogFinal = conf["PathFileLog"] + "/" + conf["FileLogFinal"];

            // test si le fichier fileLogFinal est inferieur a une taille max
            if (File.Exists(@fileLogFinal))
            {
                long lengthFile = new System.IO.FileInfo(@fileLogFinal).Length;
                long sizeLog = Convert.ToInt32(conf["SizeLog"]);
                if (lengthFile >= sizeLog * 1024)
                    LogToFile.ArchiveLog(conf["PathFileLog"], conf["FileLogFinal"]);
            }

            // logtemp exist => delete and create
            string fileLogTemp = conf["PathFileLog"] + "/" + conf["FileLogTemp"];
            if (File.Exists(@fileLogTemp))
            {
                File.Delete(@fileLogTemp);
            }
            LogToFile.WriteLog(conf["PathFileLog"], conf["FileLogTemp"], $"{new String('+', 10)} debut de la sauvegarde le " +
                $"{DateTime.Now.ToLongDateString()} à {DateTime.Now.ToLongTimeString()} ");

            // pidFile n'existe pas, on le créé et on continu
            using (StreamWriter pidFile = File.CreateText(pidFileName))
            {
                pidFile.Close();
            }

            // validation du serveur ftp, on sauvegarde les switchs
            Uri uri = new Uri("ftp://" + conf["FtpAdresseIp"]);
            if (Check.CheckFtpServeur(uri, conf["FtpUser"], conf["FtpPassword"]))
            {
                // validation des mots de passe passwordSwitch et passwordEn
                string passwordSwitch = Aes.DecryptString(conf["PasswordSwitch"], Sel.Val);
                string passwordEn = Aes.DecryptString(conf["PasswordEn"], Sel.Val);
                // si le retour de la fonction encrypt est une chaine vide, on sort
                if (passwordSwitch.Equals("") || passwordEn.Equals(""))
                {
                    LogToFile.WriteLog(conf["pathFileLog"], conf["FileLogTemp"], "ERREUR : mot de passe incorect");
                    if (backupStatus)
                        backupStatus = false;
                }
                // sinon on continu
                else
                {
                    // affecte la section des switchs
                    ListeSwitchSection sectionSwitch = (ListeSwitchSection)ConfigurationManager.GetSection("ListeSwitchsSection");

                    // log du demarrage de la sauvegarde
                    foreach (Switch switchElement in sectionSwitch.Listes)
                    {
                        // on recupere le nom du switch et son adresse IP
                        string switchName = switchElement.SwitchName;
                        string switchIp = switchElement.SwitchIp;
                        // validation de l'adresse du switch
                        if (!Check.VerifAdresseIp(switchIp))
                        {
                            // si on valide on sort
                            LogToFile.WriteLog(conf["PathFileLog"], conf["FileLogTemp"], "Switch " + switchName + "(" + switchIp + ")" + " : Erreur adresse Ip \r\n");
                            if (backupStatus)
                                backupStatus = false;
                        }
                        // test ping switch ne repond pas
                        else if (!Check.PingIp(switchIp))
                        {
                            // ping non  valide on sort
                            LogToFile.WriteLog(conf["PathFileLog"], conf["FileLogTemp"], "Switch " + switchName + "(" + switchIp + ")" + " : ne repond pas au ping \r\n");
                            if (backupStatus)
                                backupStatus = false;
                        }
                        // données validées, on envoi toutes les info sur l'instance cisco
                        else
                        {
                            LogToFile.WriteLog(conf["pathFileLog"], conf["FileLogTemp"], $"{ new string(' ', 11)}{ new string('-', 10)}" +
                                " debut sauvegarde de " + switchName + "(" + switchIp + ")");
                            Cisco cisco = new Cisco(conf, switchName, switchIp, Sel.Val);
                            LogToFile.WriteLog(conf["PathFileLog"], conf["FileLogTemp"], $"{new String(' ', 10)} etat du backup de sauvegarde du switch => " + cisco.BackupState);
                            LogToFile.WriteLog(conf["pathFileLog"], conf["FileLogTemp"], $"{ new string(' ', 11)}{ new string('-', 10)}" +
                                    " fin sauvegarde de " + switchName + "(" + switchIp + ")\r\n");

                            //Console.WriteLine("etat du backup de sauvegarde du switch => " + cisco.BackupState);
                            if (backupStatus)
                                backupStatus = cisco.BackupState;
                        }
                    }
                }
            }
            // serveur ftp non valide
            else
            {
                LogToFile.WriteLog(conf["PathFileLog"], conf["FileLogTemp"], "Erreur serveur Ftp : " + conf["FtpAdresseIp"] + (char)13);
                if (backupStatus)
                    backupStatus = false;
            }

            // enregistrement de l'etat du backup dans le log
            LogToFile.WriteLog(conf["PathFileLog"], conf["FileLogTemp"], "Etat du backup general => " + backupStatus);

            //stop timestamp
            LogToFile.WriteLog(conf["PathFileLog"], conf["FileLogTemp"], $"{new String('+', 10)} " +
               $"fin de la sauvegarde le {DateTime.Now.ToLongDateString()} à {DateTime.Now.ToLongTimeString()} ");

            // recuperation de la liste des @ mails
            Hashtable sectionListeMail = (Hashtable)ConfigurationManager.GetSection("ListeMail");
            // envoi du mail de log
#if !DEBUG
                    SendMail sendMail = new SendMail(sectionListeMail, conf["PathFileLog"], conf["FileLogTemp"], conf["SmtpServeur"], conf["SenderFrom"], backupStatus);
#endif
            // copie du log temporaire dans le log final
            LogToFile.AppendShortToFinalLog(conf["PathFileLog"], conf["fileLogTemp"], conf["FileLogFinal"]);

            //if (backupStatus)
            File.Delete(pidFileName);

        }

        // creation du dictionnaire des parmatres
        private static void GetConf()
        {
            // recuperation du path du fichier log dans appSetting de l'adresse smtp et du sender 
            conf["PathFileLog"] = ConfigurationManager.AppSettings["PathFileLog"];
            conf["FileLogTemp"] = ConfigurationManager.AppSettings["FileLogTemp"];
            conf["FileLogFinal"] = ConfigurationManager.AppSettings["FileLogFinal"];
            conf["SizeLog"] = ConfigurationManager.AppSettings["SizeLog"];
            conf["SmtpServeur"] = ConfigurationManager.AppSettings["SmtpServeur"];
            conf["SenderFrom"] = ConfigurationManager.AppSettings["SenderFrom"];
            conf["FileLogPidFile"] = "PidFile.log";

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
                    Console.WriteLine("=> SovSwitch /encrypt mot de passe (pour encrypter un mot de passe)");
                    Console.WriteLine("=> SovSwitch /backup (pour sauvegarder une liste de switchs");
                    break;

                default:
                    Console.WriteLine("Default case");
                    break;
            }
        }
    }
}

