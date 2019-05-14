# SteamIDResolver
Current User's Steam ID and Username Resolver

When you logged in to Steam, it will retrieve your current user ID from the client.
To use this library, you have to get your API key from Steam Developers: https://steamcommunity.com/dev/apikey


## Usages
```
var steam = new SteamBridge();
Console.WriteLine("[+] Getting Current Steam User's ID...");
Console.WriteLine();

Console.WriteLine("STEAMID64: " + steam.GetSteamId().ToString(CultureInfo.InvariantCulture));
Console.WriteLine("STEAMID32: " + SteamIDConvert.Steam64ToSteam2((long)steam.GetSteamId()));

Console.WriteLine();
Console.WriteLine("[+] Searching Username from ID...");
Console.WriteLine();
Console.WriteLine("Username: " + SteamUserSearcher.GetUserName(steam.GetSteamId().ToString(CultureInfo.InvariantCulture), "Your Steam ID"));
Console.ReadKey();
```
