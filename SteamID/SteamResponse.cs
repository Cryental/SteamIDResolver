﻿using System.Collections.Generic;
using Newtonsoft.Json;

namespace SteamID
{
    public class Player
    {
        [JsonProperty("steamid")] public string Steamid { get; set; }

        [JsonProperty("communityvisibilitystate")]
        public int Communityvisibilitystate { get; set; }

        [JsonProperty("profilestate")] public int Profilestate { get; set; }

        [JsonProperty("personaname")] public string Personaname { get; set; }

        [JsonProperty("lastlogoff")] public int Lastlogoff { get; set; }

        [JsonProperty("commentpermission")] public int Commentpermission { get; set; }

        [JsonProperty("profileurl")] public string Profileurl { get; set; }

        [JsonProperty("avatar")] public string Avatar { get; set; }

        [JsonProperty("avatarmedium")] public string Avatarmedium { get; set; }

        [JsonProperty("avatarfull")] public string Avatarfull { get; set; }

        [JsonProperty("personastate")] public int Personastate { get; set; }

        [JsonProperty("primaryclanid")] public string Primaryclanid { get; set; }

        [JsonProperty("timecreated")] public int Timecreated { get; set; }

        [JsonProperty("personastateflags")] public int Personastateflags { get; set; }

        [JsonProperty("loccountrycode")] public string Loccountrycode { get; set; }

        [JsonProperty("locstatecode")] public string Locstatecode { get; set; }

        [JsonProperty("loccityid")] public int Loccityid { get; set; }
    }

    public class Response
    {
        [JsonProperty("players")] public IList<Player> Players { get; set; }
    }

    public class SteamResponse
    {
        [JsonProperty("response")] public Response Response { get; set; }
    }
}