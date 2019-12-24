using System;
using System.Text;
using System.Net.Sockets;
using CryptoLibrary;
using System.Collections.Specialized;
using LogLibrary;
using System.IO;

namespace SovSwitch
{
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

            char[] trimChar = new char[] { '\0' };
            
            var rxDatas = new[]{new {rData="Username",sData="monitor"},
                                new {rData="Password",sData=Aes.DecryptString(conf["passwordSwitch"],sel)},
                                new {rData=">",sData="en"},
                                new {rData="Password",sData=Aes.DecryptString(conf["passwordEn"],sel)},
                                new {rData="#",sData="copy running-config ftp"},
                                new {rData="Address",sData=conf["FTpAdresseIp"]},
                                new {rData="Destination",sData=conf["ftpSuffix"] + switchName + ".txt"},
                                new {rData="copied",sData="exit"},
                                };

            double i = 0;
            try
            {
                client = new TcpClient(switchIp, port);
                stream = client.GetStream();
                stream.ReadTimeout = 10000;


                LogToFile.Log(conf["pathFileLog"],conf["FileLogTemp"], "------ debut sauvegarde de " + switchName);
                foreach (var n in rxDatas)
                {
                    try
                    {
                        WaitForData(n.rData, ref rep);
                        res += rep.Trim('\0');
                        rep = "";
                        SendData(n.sData);
                    }
                    catch
                    {
                        LogToFile.Log(conf["pathFileLog"], conf["FileLogTemp"], "ERREUR sur la sauvegarde de " + switchName);
                        Console.WriteLine("ERREUR sur la sauvegarde de " + switchName);
                    }
                }
                i++;
                stream.Close();
                LogToFile.Log(conf["pathFileLog"], conf["FileLogTemp"], "------ fin sauvegarde de " + switchName);
                LogToFile.Log(conf["pathFileLog"], conf["FileLogTemp"],"\r\n");
            }
            catch
            {
                LogToFile.Log(conf["pathFileLog"], conf["FileLogTemp"],switchName + "(" + switchIp + ")" + " : ne semble pas etre un switch");
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
            //Console.WriteLine(comp);
            //Console.WriteLine(page);
            string strTemp = page.Trim(new Char[] { '\r','\n','\t' });
            LogToFile.Log(conf["pathFileLog"], conf["FileLogTemp"], "\t" + strTemp);
            Console.WriteLine("\t" + strTemp);
            //Console.WriteLine("-");
            //Console.WriteLine("\n");
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





