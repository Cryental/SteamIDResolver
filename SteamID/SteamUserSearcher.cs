using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace SteamID
{
    public static class SteamUserSearcher
    {
        public static string GetUserName(string steamID64, string Key)
        {
            WebClient webClient = new WebClient();
            webClient.Proxy = null;
            string response = webClient.DownloadString(String.Format("https://api.steampowered.com/ISteamUser/GetPlayerSummaries/v0002/?key={0}&steamids={1}", Key, steamID64));
            var responseparse = JsonConvert.DeserializeObject<SteamResponse>(response);

            return responseparse.Response.Players[0].Personaname;
        }
    }
}
