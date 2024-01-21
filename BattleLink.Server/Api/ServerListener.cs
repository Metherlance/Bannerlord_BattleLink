using BattleLink.Common;
using BattleLink.Common.Utils;
using RealmsBattle.Server;
using System;
using System.Globalization;
using System.IO;
using System.Net;
using System.Threading.Tasks;
using TaleWorlds.Core;
using TaleWorlds.Engine;
using TaleWorlds.Library;
using TaleWorlds.MountAndBlade;
using static TaleWorlds.Library.Debug;
using static TaleWorlds.MountAndBlade.MultiplayerOptions;

namespace BattleLink.Server.Api
{
    public class ServerListener
    {
        private static HttpListener listener;

        public static void Start()
        {
            _ = ServerApi();
        }

        public static void Stop()
        {
            if (listener != null)
            {
                listener.Close();
            }
        }


        static void ProcessRequest(HttpListenerContext context)
        {
            string clientIP = context.Request.RemoteEndPoint.ToString();
            MBDebug.Print($"BL Api, clientIP: {clientIP} {context.Request.HttpMethod}", 0, DebugColor.Green);
            if (context.Request.HttpMethod == "POST")
            {
                try
                {
                    string filename = context.Request.Headers["File-Name"];
                    if (filename.IsEmpty())
                    {
                        MBDebug.Print($"File name empty", 0, DebugColor.Red);
                        context.Response.StatusCode = 400;//dont give info
                        context.Response.Close();
                        return;
                    }
                    if (filename.Contains("..") || filename.Contains("/") || filename.Contains("\\"))
                    {
                        MBDebug.Print($"Injection!!! {filename}", 0, DebugColor.Red);
                        context.Response.StatusCode = 400;//dont give info
                        context.Response.Close();
                        return;
                    }

                    string date = context.Request.Headers["x-ms-date"];
                    var dateTime = DateTimeOffset.ParseExact(date, "o", CultureInfo.InvariantCulture);
                    // 10 seconds tolerance
                    if (date.IsEmpty() || dateTime.AddSeconds(15) < DateTimeOffset.UtcNow || dateTime > DateTimeOffset.UtcNow)
                    {
                        // too long ago
                        MBDebug.Print($"Peer not sync date {date}", 0, DebugColor.Red);
                        context.Response.StatusCode = 400;//dont give info
                        context.Response.Close();
                        return;
                    }

                    string contentInitilizerXml;
                    using (StreamReader reader = new StreamReader(context.Request.InputStream))
                    {
                        contentInitilizerXml = reader.ReadToEnd();
                    }

                    if (contentInitilizerXml.Length > 512 * 1024)
                    {
                        MBDebug.Print($"File too long {contentInitilizerXml.Length}", 0, DebugColor.Red);
                        context.Response.StatusCode = 400;//dont give info
                        context.Response.Close();
                        return;
                    }

                    string secret = new PropertiesUtils(System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink", "config.properties")).Get("secret");
                    string signature = context.Request.Headers["signature"];
                    string signatureCalculated = SignHttp.ComputeSignature(date + contentInitilizerXml + secret);
                    if (!signatureCalculated.Equals(signature))
                    {
                        MBDebug.Print($"Wrong signature excepted: {signatureCalculated}, recevied: {signature}", 0, DebugColor.Red);
                        context.Response.StatusCode = 400;//dont give info
                        context.Response.Close();
                        return;
                    }

                    // Change the path where you want to save the uploaded file
                    string filePath = System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink", "Battles", "Pending", filename);

                    // Save the content to a file
                    File.WriteAllText(filePath, contentInitilizerXml);

                    MBDebug.Print($"File saved to: {filePath}", 0, DebugColor.Green);

                    // Respond to the client
                    context.Response.StatusCode = 200;
                    context.Response.Close();

                    // set new battle
                    MultiplayerOptions.Instance.GetOptionFromOptionType(OptionType.Map, MultiplayerOptionsAccessMode.CurrentMapOptions).GetValue(out string gameMode);
                    if (!"BattleLink".Equals(gameMode))
                    {
                        _ = BattleLinkGameMode.EndAndSetNextMission();
                    }

                    //if(1==new DirectoryInfo(System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink", "Battles", "Pending")).GetFiles("BL_MPBattle_*_Initializer.xml").Length)
                    //{
                    //    _ = EndAndStartMission(sceneName);

                    //    BLMissionMpDomination.setNextBLMap();
                    //}
                }
                catch (Exception ex)
                {
                    MBDebug.Print($"Error: {ex.Message}", 0, DebugColor.Red);
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
            }
            else if (context.Request.HttpMethod == "GET")
            {
                try
                {
                    string filename = context.Request.Headers["File-Name"];
                    if (filename.IsEmpty())
                    {
                        MBDebug.Print($"File name empty", 0, DebugColor.Red);
                        context.Response.StatusCode = 400;//dont give info
                        context.Response.Close();
                        return;
                    }
                    if (filename.Contains("..") || filename.Contains("/") || filename.Contains("\\"))
                    {
                        MBDebug.Print($"Injection!!! {filename}", 0, DebugColor.Red);
                        context.Response.StatusCode = 400;//dont give info
                        context.Response.Close();
                        return;
                    }
                    // Change the path where you want to save the uploaded file
                    string filePath = System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink", "Battles", "Finished", filename);
                    if (!File.Exists(filePath))
                    {
                        MBDebug.Print($"File name didn t exist", 0, DebugColor.Red);
                        context.Response.StatusCode = 400;//dont give info
                        context.Response.Close();
                        return;
                    }

                    // Save the content to a file
                    byte[] buffer = File.ReadAllBytes(filePath);

                    MBDebug.Print($"Send file: {filePath}", 0, DebugColor.Green);

                    // Respond to the client

                    // Get a response stream and write the response to it.
                    context.Response.ContentType = "application/octet-stream";
                    context.Response.ContentLength64 = buffer.Length;
                    context.Response.OutputStream.Write(buffer, 0, buffer.Length);
                    context.Response.OutputStream.Close();
                    context.Response.StatusCode = 200;
                    context.Response.Close();
                }
                catch (Exception ex)
                {
                    MBDebug.Print($"Error: {ex.Message}", 0, DebugColor.Red);
                    context.Response.StatusCode = 500;
                    context.Response.Close();
                }
            }
            else
            {
                context.Response.StatusCode = 405; // Method Not Allowed
                context.Response.Close();
            }
        }

        static async Task ServerApi()
        {
            if (!HttpListener.IsSupported)
            {
                MBDebug.Print("Windows XP SP2 or Server 2003 is required to use the HttpListener class.", 0, DebugColor.Red);
                return;
            }
            string secret = new PropertiesUtils(System.IO.Path.Combine(BasePath.Name, "Modules", "BattleLink", "config.properties")).Get("secret");
            if (secret.IsEmpty() || secret.Length < 36)
            {
                MBDebug.Print("Secret not set in config.properties or too short " + secret, 0, DebugColor.Red);
                return;
            }

            var baseUrls = new string[] { "http://localhost:7211/battlelink/api/battles/" };
            // Create a listener.
            using (listener = new HttpListener())
            {
                // Add the prefixes.
                foreach (string s in baseUrls)
                {
                    listener.Prefixes.Add(s);
                }

                listener.Start();
                MBDebug.Print("Server is listening on " + baseUrls, 0, DebugColor.Green);

                while (true)
                {
                    HttpListenerContext context = await listener.GetContextAsync();
                    ProcessRequest(context);
                }
            }
        }
    }
}
