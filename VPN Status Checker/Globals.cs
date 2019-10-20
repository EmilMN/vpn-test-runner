using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Web;

namespace VPN_Status_Checker
{
    static class Globals
    {

        public static RootObject account;
        public static String report;
        public static int testsRun = 0;

        public static String timeOut = "TimedOut";
        public static String success = "Success";

        public static int downServers = 0;
        public static int domainsNotResponding = 0;

    }

    public class util { 
    public static string getResponse(string url)
    {
        var client = new WebClient();

            try
            {
                var response = client.DownloadString(url);
                return response;

            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

       public static string currentDateTime()
        {
            // Make sure the reports include the reference date and time

            String today = DateTime.Now.ToString("yyyy-MM-dd hh-mm-ss");
            return today;
        }

    }

}