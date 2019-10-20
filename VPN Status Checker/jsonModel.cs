using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace VPN_Status_Checker
{

    public class RootObject
    {
        [JsonProperty("LogicalServers")]
        public List<LogicalServers> LogicalServers { get; set; }

        [JsonProperty("Code")]
        public String Code { get; set; }
    }

    public class LogicalServers
    {
        [JsonProperty("Name")]

        public String Name { get; set; }

        [JsonProperty("EntryCountry")]

        public String EntryCountry { get; set; }

        [JsonProperty("ExitCountry")]
        public String ExitCountry { get; set; }

        [JsonProperty("Domain")]
        public String Domain { get; set; }

        [JsonProperty("Tier")]
        public int Tier { get; set; }

        [JsonProperty("Features")]
        public int Features { get; set; }

        [JsonProperty("Region")]
        public String Region { get; set; }

        [JsonProperty("City")]
        public String City { get; set; }

        [JsonProperty("ID")]
        public String ID { get; set; }

        [JsonProperty("Status")]
        public int Status { get; set; }

        [JsonProperty("Load")]
        public int Load { get; set; }

        [JsonProperty("Score")]
        public float Score { get; set; }

        [JsonProperty("Servers")]
        public List<Servers> Servers { get; set; }

        [JsonProperty("Location")]
        public Location Location { get; set; }
    }

    public class Servers
    {
        [JsonProperty("EntryIP")]
        public string EntryIP { get; set; }

        [JsonProperty("ExitIP")]
        public string ExitIP { get; set; }

        [JsonProperty("Domain")]
        public string Domain { get; set; }

        [JsonProperty("ID")]
        public string ID { get; set; }

        [JsonProperty("Status")]
        public int Status { get; set; }
    }

    public class Location
    {
        [JsonProperty("Lat")]
        public double Lat { get; set; }
        [JsonProperty("Long")]
        public double Long { get; set; }

    }

}