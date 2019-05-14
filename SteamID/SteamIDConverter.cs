using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace SteamID
{
    public enum AuthIdType
    {
        AuthId_Engine = 0,
        AuthId_Steam2,
        AuthId_Steam3,
        AuthId_SteamID64,
    };

    public class SteamIDRegex
    {
        public const string Steam2Regex = "^STEAM_0:[0-1]:([0-9]{1,10})$";
        public const string Steam32Regex = "^U:1:([0-9]{1,10})$";
        public const string Steam64Regex = "^7656119([0-9]{10})$";
    }

    public static class SteamIDConvert
    {
        public static string Steam32ToSteam2(string input)
        {
            if (!Regex.IsMatch(input, SteamIDRegex.Steam32Regex))
            {
                return string.Empty;
            }
            long steam64 = Steam32ToSteam64(input);
            return Steam64ToSteam2(steam64);
        }

        public static string Steam2ToSteam32(string input)
        {
            if (!Regex.IsMatch(input, SteamIDRegex.Steam2Regex))
            {
                return string.Empty;
            }
            long steam64 = Steam2ToSteam64(input);
            return Steam64ToSteam32(steam64);
        }

        public static long Steam32ToSteam64(string input)
        {
            long steam32 = Convert.ToInt64(input.Substring(4));
            if (steam32 < 1L || !Regex.IsMatch("U:1:" + steam32.ToString((IFormatProvider)CultureInfo.InvariantCulture), "^U:1:([0-9]{1,10})$"))
            {
                return 0;
            }
            return steam32 + 76561197960265728L;
        }

        public static string Steam64ToSteam2(long communityId)
        {
            if (communityId < 76561197960265729L || !Regex.IsMatch(communityId.ToString((IFormatProvider)CultureInfo.InvariantCulture), "^7656119([0-9]{10})$"))
                return string.Empty;
            communityId -= 76561197960265728L;
            long num = communityId % 2L;
            communityId -= num;
            string input = string.Format("STEAM_0:{0}:{1}", num, (communityId / 2L));
            if (!Regex.IsMatch(input, "^STEAM_0:[0-1]:([0-9]{1,10})$"))
            {
                return string.Empty;
            }
            return input;
        }

        public static long Steam2ToSteam64(string accountId)
        {
            if (!Regex.IsMatch(accountId, "^STEAM_0:[0-1]:([0-9]{1,10})$"))
            {
                return 0;
            }
            return 76561197960265728L + Convert.ToInt64(accountId.Substring(10)) * 2L + Convert.ToInt64(accountId.Substring(8, 1));
        }

        public static string Steam64ToSteam32(long communityId)
        {
            if (communityId < 76561197960265729L || !Regex.IsMatch(communityId.ToString((IFormatProvider)CultureInfo.InvariantCulture), "^7656119([0-9]{10})$"))
            {
                return string.Empty;
            }
            return string.Format("U:1:{0}", communityId - 76561197960265728L);
        }

    }

    public class SteamID_Engine
    {
        public string WorkingID { get; private set; }
        public AuthIdType AuthType { private set; get; }
        public string Steam2 { private set; get; }
        public string Steam32 { private set; get; }
        public long Steam64 { private set; get; }

        public SteamID_Engine(string ID)
        {
            WorkingID = ID;
            if (Regex.IsMatch(WorkingID, SteamIDRegex.Steam2Regex))
            {
                AuthType = AuthIdType.AuthId_Steam2;
                Steam2 = WorkingID;
                Steam32 = SteamIDConvert.Steam2ToSteam32(WorkingID);
                Steam64 = SteamIDConvert.Steam2ToSteam64(WorkingID);
            }
            else if (Regex.IsMatch(WorkingID, SteamIDRegex.Steam32Regex))
            {
                AuthType = AuthIdType.AuthId_Steam3;
                Steam2 = SteamIDConvert.Steam32ToSteam2(WorkingID);
                Steam32 = WorkingID;
                Steam64 = SteamIDConvert.Steam32ToSteam64(WorkingID);
            }
            else if (Regex.IsMatch(WorkingID, SteamIDRegex.Steam64Regex))
            {
                AuthType = AuthIdType.AuthId_SteamID64;
                Steam2 = SteamIDConvert.Steam64ToSteam2(Int64.Parse(WorkingID));
                Steam32 = SteamIDConvert.Steam64ToSteam32(Int64.Parse(WorkingID));
                Steam64 = Int64.Parse(WorkingID);
            }
            else
            {
                AuthType = AuthIdType.AuthId_Engine;
            }
        }

        public string AuthType_string()
        {
            if (AuthType == AuthIdType.AuthId_Steam2)
            {
                return "SteamID2";
            }
            else if (AuthType == AuthIdType.AuthId_Steam3)
            {
                return "SteamID32";
            }
            else if (AuthType == AuthIdType.AuthId_SteamID64)
            {
                return "SteamID64";
            }
            else if (AuthType == AuthIdType.AuthId_Engine)
            {
                return "Engine ID";
            }
            else
            {
                return string.Empty;
            }
        }
    }
}