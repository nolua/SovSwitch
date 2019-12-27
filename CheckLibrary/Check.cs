using CryptoLibrary;
using System;
using System.Collections.Specialized;
using System.Net;
using System.Net.NetworkInformation;
using System.Text.RegularExpressions;

namespace CheckLibrary
{
    public class Check
    {
        public static bool VerifAdresseIp(string adresse)
        {
            //string modele1 = "^\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}\\.\\d{1,3}$";
            string modele1 = "^(([01]?\\d\\d?|2[0-4]\\d|25[0-5])\\.){3}([01]?\\d\\d?|2[0-4]\\d|25[0-5])$";
            Regex regex1 = new Regex(modele1);
            if (regex1.IsMatch(adresse) && !adresse.Equals("0.0.0.0")) 
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public static bool PingIp(string adresseIp)
        {
            bool result = false;
            Int32 timeout = 10000;

            Ping PingServeur = new Ping();
            try
            {
                PingReply ReponseServeur = PingServeur.Send(adresseIp,timeout);
                if (ReponseServeur.Status == IPStatus.Success)
                    result = true;
            }
            catch { }
            return result;
        }

        public static bool CheckFtpServeur(Uri serverUri,string ftpUser,string ftpPassword)
        {
            bool result = false;
            if (serverUri.Scheme == Uri.UriSchemeFtp)
            {
                // Get the object used to communicate with the server.
                FtpWebRequest request = (FtpWebRequest)WebRequest.Create(serverUri);
                request.Method = WebRequestMethods.Ftp.ListDirectory;
                request.Credentials = new NetworkCredential(ftpUser, Aes.DecryptString(ftpPassword,"louna"));
                try
                {
                    FtpWebResponse response = (FtpWebResponse)request.GetResponse();
                    response.Close();
                    result = true;
                }
                catch { }
            }
            return result;
        }
    }
}
