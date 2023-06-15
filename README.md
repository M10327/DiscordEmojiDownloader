# Discord Emoji and Sticker Downloader
This is a lightweight commandline application for downloading nearly any emoji and stickers from any discord server you are in. Does *not* download lottie stickers, but when I downloaded all stickers from the 60+ servers I was in there was only 1 lottie sticker out of all of them. This requires you provide your discord token, but it shouldnt (no guarantee though, you take the risk by using this) get your account banned, beause it only uses your token as authentication in a GET request to `https://discordapp.com/api/v6/guilds/yourserveridhere` to download the information from the server id you provide. If you run into any problems, please make a post in the [issues](https://github.com/M10327/DiscordEmojiDownloader/issues) section. Supports downloading just 1 server at a time, or queueing up many servers through an `input.txt` file. 

### Requirements
- .NET 6.0 or higher
- [apng2gif v1.8 NON-GUI](https://sourceforge.net/projects/apng2gif/files/1.8/)
- Being in the discord server you want to download emojis from
- Windows

### How to use
1. Download the newest [release](https://github.com/M10327/DiscordEmojiDownloader/releases)
2. Extract the zip file somewhere
3. Download apng2gif and extract that exe into the same folder as `DiscordEmojiDownloader.exe`
4. Run `DiscordEmojiDownloader.exe` and follow onscreen instructions