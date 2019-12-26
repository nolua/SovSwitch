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

        private string sel = "louna";
        private string switchName;
        private string switchIp;
        private NameValueCollection conf = new NameValueCollection();
        
        public Cisco() { }

        public Cisco(params Object[] arg)
        {
            conf = (NameValueCollection)arg[0];
            string res = "";
            string rep = "";
            switchName = (string)arg[1];
            switchIp = (string)arg[2];
            double i = 0;
            String pattern = @"\r\n";


            char[] trimChar = new char[] { '\0' };
            
            var rxDatas = new[]{new {rData="Username",sData=conf["Username"]},
                                new {rData="Password",sData=Aes.DecryptString(conf["passwordSwitch"],sel)},
                                new {rData=">",sData="en"},
                                new {rData="Password",sData=Aes.DecryptString(conf["passwordEn"],sel)},
                                new {rData="#",sData="copy running-config ftp"},
                                new {rData="Address",sData=conf["FTpAdresseIp"]},
                                new {rData="Destination",sData=conf["ftpSuffix"] + switchName + ".txt"},
                                new {rData="copied",sData="exit"},
                                };

            try
            {
                client = new TcpClient(switchIp, port);
                stream = client.GetStream();
                stream.ReadTimeout = 10000;


                LogToFile.LogAppend(conf["pathFileLog"],conf["FileLogTemp"], "\t------ debut sauvegarde de " + switchName);
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
                                LogToFile.LogAppend(conf["pathFileLog"], conf["FileLogTemp"], '\t' + element);
                                Console.WriteLine('\t' + element);
                            }
                        rep = "";
                        SendData(n.sData);
                        
                    }
                    catch (Exception e)
                    {
                        //Console.WriteLine("Exception: " + e.Message);
                        LogToFile.LogAppend(conf["pathFileLog"], conf["FileLogTemp"], "ERREUR sur la sauvegarde de " + switchName);
                        Console.WriteLine("ERREUR sur la sauvegarde de " + switchName);
                    }
                }
                i++;
                stream.Close();
                client.Close();

                Console.WriteLine(res);
                LogToFile.LogAppend(conf["pathFileLog"], conf["FileLogTemp"], "\t------ fin sauvegarde de " + switchName);
                LogToFile.LogAppend(conf["pathFileLog"], conf["FileLogTemp"],"\n");
                //LogToFile.Log("c:/temp/SovSwitch",switchName+".log",res);
            }
            catch (Exception e)
            {
                LogToFile.LogAppend(conf["pathFileLog"], conf["FileLogTemp"],switchName + "(" + switchIp + ")" + " : ne semble pas etre un switch");
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





