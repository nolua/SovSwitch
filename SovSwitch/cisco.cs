using System;
using System.Text;
using System.Net.Sockets;
using CryptoLibrary;
using System.Collections.Specialized;
using LogLibrary;
using System.IO;
using System.Collections.Generic;

namespace SovSwitch
{
    /// <summary>
    /// crednetiel utilisé pour la version de test :
    ///  ftp : fx/cenexi
    ///  switch:
    ///  fujitsu/1formatic
    ///  en:cenexi
    /// </summary>
    class Cisco
    {
        private int port = 23;
        TcpClient client;
        NetworkStream stream;

        //private string sel = "louna";
        private string switchName;
        private string switchIp;
        private NameValueCollection conf = new NameValueCollection();
        private bool backupState=true;

        public bool BackupState { get => backupState; set => backupState = value; }

        public Cisco() { }

        public Cisco(params Object[] arg)
        {
            conf = (NameValueCollection)arg[0];
            //string res = "";
            
            switchName = (string)arg[1];
            switchIp = (string)arg[2];
            string sel = (string)arg[3];
            string pattern = @"\r\n";
            string rep = "";
            int i = 0; // pour test plantage com tcp

            char[] trimChar = new char[] { '\0' };
            
            var rxDatas = new[]{new {rData="Username",sData=conf["Username"]},
                                new {rData="Password",sData=Aes.DecryptString(conf["PasswordSwitch"],sel)},
                                new {rData=">",sData="en"},
                                new {rData="Password",sData=Aes.DecryptString(conf["PasswordEn"],sel)},
                                new {rData="#",sData="copy running-config ftp:"},
                                new {rData="Address",sData=conf["FTpAdresseIp"]},
                                new {rData="Destination",sData=conf["ftpSuffix"] + switchName + ".txt"},
                                new {rData="copied",sData="exit"},
                                };

            try
            {
                client = new TcpClient(switchIp, port);
                stream = client.GetStream();
                stream.ReadTimeout = 30000;

                Console.WriteLine();
                LogToFile.LogAppend(conf["pathFileLog"],conf["FileLogTemp"], "\t------ debut sauvegarde de " + switchName + "(" + switchIp + ")");
                Console.WriteLine("\t------ debut sauvegarde de " + switchName + "(" + switchIp + ")");
                foreach (var n in rxDatas)
                {
                    try
                    {
                        WaitForData(n.rData, ref rep);
                        String[] elements = System.Text.RegularExpressions.Regex.Split(rep, pattern);
                        elements = System.Text.RegularExpressions.Regex.Split(rep,pattern);
                        foreach (string element in elements)
                            if (!element.Equals(""))
                            {
                                LogToFile.LogAppend(conf["pathFileLog"], conf["FileLogTemp"], "\t\t" + element);
                                Console.WriteLine("\t\t" + element);
                            }
                        rep = "";
                        SendData(n.sData);

                        //test plantage com tcp
                        if (i < 0)
                        {
                            stream.Close();
                            client.Close();
                        }
                        i++;
                        // fin test plantage com tcp
                    }
                    catch (Exception e)
                    {
                        // on positionne l'etat du backup à false
                        backupState = false;
                        Console.WriteLine();
                        Console.WriteLine("Exception: " + e.Message);
                        LogToFile.LogAppend(conf["pathFileLog"], conf["FileLogTemp"], e.Message + '\n' + "ERREUR sur la sauvegarde de " + switchName + "(" + switchIp + ")");
                        Console.WriteLine("ERREUR sur la sauvegarde de " + switchName + "(" + switchIp + ")");
                    }
                }
                //i++;
                stream.Close();
                client.Close();

                //Console.WriteLine(res);
                LogToFile.LogAppend(conf["pathFileLog"], conf["FileLogTemp"], "\t------ fin sauvegarde de " + switchName + "(" + switchIp + ")");
                Console.WriteLine("\t------ fin sauvegarde de " + switchName + "(" + switchIp + ")");
                LogToFile.LogAppend(conf["pathFileLog"], conf["FileLogTemp"],"\n");
                //LogToFile.Log("c:/temp/SovSwitch",switchName+".log",res);
            }
            catch (Exception e)
            {
                // on positionne l'etat du backup à false
                backupState = false;
                Console.WriteLine();
                Console.WriteLine("Exception: " + e.Message);
                LogToFile.LogAppend(conf["pathFileLog"], conf["FileLogTemp"],switchName + "(" + switchIp + ")" + " : ne semble pas etre un switch\n");
                Console.WriteLine(switchName + "(" + switchIp + ")" + " : ne semble pas etre un switch");
            }
        }

        public void SendData(string msg)
        {
            Byte[] bytesSend = Encoding.UTF8.GetBytes(msg + (char)13);
            stream.Write(bytesSend, 0, bytesSend.Length);
        }

        public void WaitForData(string findRxChaine, ref string page)
        {
            int comp = 0;
            Byte[] bytesReceived = new Byte[512];
            int nbBytes;

            while (!(FindString(page, findRxChaine)))
            {
                nbBytes = stream.Read(bytesReceived, 0, bytesReceived.Length);
                page = System.Text.Encoding.ASCII.GetString(bytesReceived, 0, nbBytes);
                comp++;
            }
        }

        public bool FindString(string searchWithinThis, string searchForThis)
        {
            int firstCharacter = searchWithinThis.IndexOf(searchForThis);
            if (firstCharacter == -1)
                return false;
            else
                return true;
        }
    }
}





