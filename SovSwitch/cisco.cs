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
            //int i = 0; // pour test plantage com tcp

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
                stream.ReadTimeout = 10000;
                stream.WriteTimeout = 10000;

                Console.WriteLine();
                // open log switch
                LogToFile.LogAppend(conf["pathFileLog"],conf["FileLogTemp"], $"{ new string(' ', 11)}{ new string('-', 10)}" + 
                    " debut sauvegarde de " + switchName + "(" + switchIp + ")");
                Console.WriteLine("\t------ debut sauvegarde de " + switchName + "(" + switchIp + ")");
                foreach (var n in rxDatas)
                {
                    try
                    {
                       // Console.WriteLine("rx => {0}  tx => {1}", n.rData, rep);
                        WaitForData(n.rData, ref rep);
                        //Console.WriteLine("rx => {0}  tx => {1}", n.rData, rep);
                        string[] elements = System.Text.RegularExpressions.Regex.Split(rep, pattern);
                        elements = System.Text.RegularExpressions.Regex.Split(rep,pattern);
                        foreach (string element in elements)
                            if (!element.Equals(""))
                            {
                                LogToFile.LogAppend(conf["pathFileLog"], conf["FileLogTemp"], "\t\t" + element);
                                Console.WriteLine("\t\t" + element);
                            }
                        rep = "";
                        SendData(n.sData);

                        // test plantage com tcp
                        // de 0 a 6 en fonction de l'etpae de connexion sur le switch
                        // 
                        //if (i ==2  && (switchName.Equals("CISCO02") || switchName.Equals("CISCO15") || switchName.Equals("CISCO32")))
                        //{
                        //    stream.Close();
                        //    client.Close();
                        //}
                        //i++;
                        // fin test plantage com tcp
                    }
                    catch (Exception ex)
                    {
                        // on positionne l'etat du backup à false
                        backupState = false;
                        Console.WriteLine();
                        Console.WriteLine("Exception: " + ex.Message);
                        Console.WriteLine("ERREUR sur la sauvegarde de " + switchName + "(" + switchIp + ")");
                        //Console.WriteLine("Exception: " + ex);
                        //LogToFile.LogAppend(conf["pathFileLog"], conf["FileLogTemp"], ex.Message + '\n' + "ERREUR sur la sauvegarde de " + switchName + "(" + switchIp + ")");
                        LogToFile.LogAppend(conf["pathFileLog"], conf["FileLogTemp"], ex.Message + "ERREUR sur la sauvegarde de " + switchName + "(" + switchIp + ")\n");
                        
                    }
                }
                //i++;
                stream.Close();
                client.Close();

                // close log switch
                LogToFile.LogAppend(conf["pathFileLog"], conf["FileLogTemp"], $"{ new string(' ', 11)}{ new string('-', 10)}" + 
                    " fin sauvegarde de " + switchName + "(" + switchIp + ")");
                //LogToFile.LogAppend(conf["pathFileLog"], conf["FileLogTemp"], "\r\n");
                Console.WriteLine("\t------ fin sauvegarde de " + switchName + "(" + switchIp + ")");
                //LogToFile.Log("c:/temp/SovSwitch",switchName+".log",res);
            }
            catch (Exception ex)
            {
                // on positionne l'etat du backup à false
                backupState = false;
                Console.WriteLine();
                Console.WriteLine("Exception: " + ex.Message);
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

            //try
            //{
                while (!(FindString(page, findRxChaine)))
                {
                    nbBytes = stream.Read(bytesReceived, 0, bytesReceived.Length);
                    page = System.Text.Encoding.ASCII.GetString(bytesReceived, 0, nbBytes);
                    comp++;
                    // pour debug
                    //Console.WriteLine("nbBtytes {0}   page={1}",nbBytes,page);
                }
            //}
            //catch (Exception ex)
            //{
            //    Console.WriteLine("erreur {0}   page={1}", ex, page);
            //}
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





