using System.Net;
using Newtonsoft.Json.Linq;
using System.Diagnostics;
using System.Text.RegularExpressions;

public class EmojiDownloader
{
    public static Task Main(string[] args) => new EmojiDownloader().MainAsync();

    public async Task MainAsync()
    {
        Console.Write("User Token: ");
        string token = "";
        while (token.Length < 1)
        {
            var input = Console.ReadLine();
            if (input is string tkn)
            {
                token = tkn;
            }
            else
                Console.WriteLine("User Token: ");
        }
        if (!File.Exists($"{Directory.GetCurrentDirectory()}\\apng2gif.exe"))
        {
            Console.WriteLine("You do not have \'apng2gif.exe\' downloaded to the root project directory!" +
                "\nPlease download it from https://sourceforge.net/projects/apng2gif/files/1.8/" +
                "\nand extract the NON GUI exe into same folder as the exe for this program");
            Console.ReadLine();
            return;
        }
        if (!Directory.Exists($"{Directory.GetCurrentDirectory()}\\attaches"))
        {
            Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}\\attaches");
        }
        Console.WriteLine("Choose how to download:" +
            "\n[1] Single Server" +
            "\n[2] Use file input (mass download)" +
            "\n[3] Close");
        switch (Console.ReadLine())
        {
            case "1":
                while (true)
                {
                    Console.Write("Server ID Please: ");
                    string guild = "";
                    while (guild.Length < 1)
                    {
                        var inp = Console.ReadLine();
                        if (inp is string tkn)
                        {
                            guild = tkn;
                        }
                        else
                            Console.WriteLine("Server ID Please: ");
                    }
                    await GetServerEmoji(token.TrimEnd(), guild);
                }
            case "2":
                if (!File.Exists($"{Directory.GetCurrentDirectory()}\\input.txt"))
                {
                    File.WriteAllText($"{Directory.GetCurrentDirectory()}\\input.txt", "Write one server id in each line. Do not leave trailing whitespace");
                    Console.WriteLine("Please close this window and fill out the \'input.txt\' file that was just generated.");
                    return;
                }
                var servers = File.ReadAllText($"{Directory.GetCurrentDirectory()}\\input.txt").Split("\n");
                foreach(var server in servers)
                {
                    await GetServerEmoji(token, server);
                }
                Console.WriteLine("===[ Finished Mass Downloading ]===");
                break;
            default:
                break;
        }
    }

    public async Task GetServerEmoji(string token, string guild)
    {
        guild = Regex.Replace(guild, "[^0-9a-zA-Z]", "");
        string response = "";
        using (var client = new HttpClient())
        {
            client.DefaultRequestHeaders.Add("Authorization", token);
            var resp = await client.GetAsync($"https://discordapp.com/api/v6/guilds/{guild}");
            Console.WriteLine($"Response: {resp.StatusCode}");
            if (resp.StatusCode != HttpStatusCode.OK)
            {
                Console.WriteLine("Unable to download emojis. Make sure you are in the server and double check inputed token and guild ids");
                return;
            }
            response = await resp.Content.ReadAsStringAsync();
        }
        var json = JObject.Parse(response);
        guild = $"{guild} - {Regex.Replace(json.SelectToken("name").ToString(), "[^0-9a-zA-Z]", "")}";
        if (Directory.Exists($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}"))
            Directory.Delete($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}", true);

        Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\emoji");
        Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\emoji_anim");
        Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\stickers");
        Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\stickers_anim");
        Directory.CreateDirectory($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\stickers_anim_apng");
        File.WriteAllText($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\output.json", response);
        Console.WriteLine($"Starting {guild}");
        // EMOJI
        var emoji = json.SelectToken("emojis");
        Console.WriteLine("Working on Emojis");
        int numEmoji = 1;
        List<string> eNames = new List<string>();
        List<string> eANames = new List<string>();
        foreach (var e in emoji)
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"Downloading emoji {numEmoji} / {emoji.Count()}");
            using (var client = new HttpClient())
            {
                string ename = Regex.Replace(e.SelectToken("name").ToString(), "[^0-9a-zA-Z]", "");
                if (ename.Length < 1) ename = e.SelectToken("id").ToString();

                if (e.SelectToken("animated").ToString() == "True")
                {
                    if (!eANames.Contains(ename, StringComparer.OrdinalIgnoreCase))
                        eANames.Add(ename);
                    else
                        ename = $"{ename} - {e.SelectToken("id")}";

                    var downloadResp = await GetImage($"https://cdn.discordapp.com/emojis/{e.SelectToken("id")}.gif?size=600&quality=lossless", ename);
                    File.WriteAllBytes($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\emoji_anim\\{ename}.gif", downloadResp);
                }
                else
                {
                    if (!eNames.Contains(ename, StringComparer.OrdinalIgnoreCase))
                        eNames.Add(ename);
                    else
                        ename = $"{ename} - {e.SelectToken("id")}";
                    var downloadResp = await GetImage($"https://cdn.discordapp.com/emojis/{e.SelectToken("id")}.webp?size=600&quality=lossless", ename);
                    File.WriteAllBytes($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\emoji\\{ename}.webp", downloadResp);
                }
            }
            numEmoji++;
        }
        Console.WriteLine();
        Console.WriteLine($"Finished downloading all emoji.");

        // STICKERS
        Console.WriteLine("Working on Stickers");
        File.Copy($"{Directory.GetCurrentDirectory()}\\apng2gif.exe", $"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\stickers_anim\\apng2gif.exe");
        int numSticker = 1;
        List<string> sNames = new List<string>();
        List<string> sANames = new List<string>();
        foreach (var s in json.SelectToken("stickers"))
        {
            Console.SetCursorPosition(0, Console.CursorTop);
            Console.Write($"Downloading sticker {numSticker} / {json.SelectToken("stickers").Count()}");
            using (var client = new HttpClient())
            {
                string sname = Regex.Replace(s.SelectToken("name").ToString(), "[^0-9a-zA-Z]", "");
                if (sname.Length < 1) sname = s.SelectToken("id").ToString();
                //else sname += $" - {s.SelectToken("id")}";

                if (s.SelectToken("format_type").ToString() == "2")
                {
                    
                    if (!sANames.Contains(sname, StringComparer.OrdinalIgnoreCase))
                        sANames.Add(sname);
                    else
                        sname = $"{sname}-{s.SelectToken("id")}";
                    
                    var downloadResp = await GetImage($"https://media.discordapp.net/stickers/{s.SelectToken("id")}.png?size=1280&passthrough=true", sname);
                    File.WriteAllBytes($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\stickers_anim\\{sname}.apng", downloadResp);
                    string sourcePath = $"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\stickers_anim";
                    string strCmdText = $"/c apng2gif.exe {sname}.apng {sname}.gif";
                    await RunApng2Gif(sourcePath, strCmdText);
                    File.Copy($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\stickers_anim\\{sname}.apng", $"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\stickers_anim_apng\\{sname}.apng");
                    File.Delete($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\stickers_anim\\{sname}.apng");
                }
                else if (s.SelectToken("format_type").ToString() == "1")
                {
                    
                    if (!sNames.Contains(sname, StringComparer.OrdinalIgnoreCase))
                        sNames.Add(sname);
                    else
                        sname = $"{sname} - {s.SelectToken("id")}";
                    
                    var downloadResp = await GetImage($"https://media.discordapp.net/stickers/{s.SelectToken("id")}.webp?size=1280", sname);
                    File.WriteAllBytes($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\stickers\\{sname}.webp", downloadResp);
                }
                else if (s.SelectToken("format_type").ToString() == "4")
                {
                    if (!sNames.Contains(sname, StringComparer.OrdinalIgnoreCase))
                        sNames.Add(sname);
                    else
                        sname = $"{sname} - {s.SelectToken("id")}";

                    var downloadResp = await GetImage($"https://media.discordapp.net/stickers/{s.SelectToken("id")}.gif?size=1280", sname);
                    File.WriteAllBytes($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\stickers_anim\\{sname}.gif", downloadResp);
                }
                else
                {
                    Console.WriteLine($"\nSkipping Sticker {sname} due to unknown file type {s.SelectToken("format_type")} (Likely a lottie image)");
                }
            }
            numSticker++;
        }
        File.Delete($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\stickers_anim\\apng2gif.exe");

        // FOLDER CLEANUP 
        if (Directory.GetFiles($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\emoji").Count() < 1)
            Directory.Delete($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\emoji");
        if (Directory.GetFiles($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\emoji_anim").Count() < 1)
            Directory.Delete($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\emoji_anim");
        if (Directory.GetFiles($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\stickers").Count() < 1)
            Directory.Delete($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\stickers");
        if (Directory.GetFiles($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\stickers_anim").Count() < 1)
            Directory.Delete($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\stickers_anim");
        if (Directory.GetFiles($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\stickers_anim_apng").Count() < 1)
            Directory.Delete($"{Directory.GetCurrentDirectory()}\\attaches\\{guild}\\stickers_anim_apng");

        Console.WriteLine();
        Console.WriteLine($"--- FINISHED DOWNLOADING SERVER {guild} ---\n\n");
        return;
    }

    public async Task<byte[]> GetImage(string url, string name)
    {
        var client = new HttpClient();
        var result = await client.GetAsync(url);
        int retryCount = 15;
        while (result.StatusCode != HttpStatusCode.OK && retryCount > 0)
        {
            await Task.Delay(250);
            result = await client.GetAsync(url);
            Console.WriteLine($"Error retrieving \'{name}\'. Retrying");
            retryCount--;
        }
        return await result.Content.ReadAsByteArrayAsync();
    }

    static Task<int> RunApng2Gif(string sourcePath, string cmd)
    {
        var tcs = new TaskCompletionSource<int>();

        var process = new Process
        {
            StartInfo = { FileName = @"C:\Windows\System32\cmd.exe",
                Verb = "runas",
                UseShellExecute = false,
                WindowStyle = ProcessWindowStyle.Hidden,
                WorkingDirectory = sourcePath,
                Arguments = cmd,
                RedirectStandardOutput = true
            },
            EnableRaisingEvents = true
        };

        process.Exited += (sender, args) =>
        {
            tcs.SetResult(process.ExitCode);
            process.Dispose();
        };

        process.Start();

        return tcs.Task;
    }

}