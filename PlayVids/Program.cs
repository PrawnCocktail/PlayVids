using HtmlAgilityPack;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace PlayVids
{
    class Program
    {
        static ProgressBar progress;

        static void Main(string[] args)
        {
            string url = "";
            //if no arguments then prompt for url and assign. no checks in place though
            if (args.Length == 0)
            {
                Console.WriteLine("Please enter a url.");
                url = Console.ReadLine().Replace("playvids.com/v/", "playvids.com/embed/");
            }
            // if arguments is equal to 1, then assume its a url, again, no checks.
            else if (args.Length == 1)
            {
                url = args[0].Replace("playvids.com/v/", "playvids.com/embed/");
            }
            //if more than one argument thow this message and close
            else
            {
                Console.WriteLine("To many arguments, please enter ONE url.");
                Console.WriteLine("Press any key to exit");
                Console.ReadLine();
                Environment.Exit(0);
            }
            

            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);

            using (var response = (HttpWebResponse)(request.GetResponse()))
            {
                using (StreamReader sr = new StreamReader(response.GetResponseStream(), Encoding.UTF8))
                {
                    Console.WriteLine("Fetching video Information...");
                    HtmlDocument doc = new HtmlDocument();
                    doc.Load(sr);
                    var vidattributes = doc.DocumentNode.SelectSingleNode("//video[@class='mediaPlayer']").GetDataAttributes();
                    var videoTitle = MakeValidFileName(doc.DocumentNode.SelectSingleNode("//a[@id='mediaPlayerTitleLink']").InnerText);

                    string m3u8Playlist = "";

                    foreach (var attribute in vidattributes)
                    {
                        //assumes resolution is in order of lowest to highest
                        if (attribute.Name.StartsWith("data-hls-src"))
                        {
                            m3u8Playlist = attribute.Value.Replace("&amp;", "&");
                        }
                    }

                    if (m3u8Playlist != "")
                    {
                        Console.WriteLine("Downloading: " + videoTitle);
                        downloadVideo(m3u8Playlist, videoTitle);
                        Console.WriteLine("Download Finished.");
                        Console.WriteLine();
                        Console.WriteLine("Press any key to exit.");
                        Console.ReadKey();
                    }
                    else
                    {
                        Console.WriteLine("No M3U8 playlist found. Cannot download video.");
                    }
                }
            }
        }

        static void downloadVideo(string playlistUrl, string videoTitle)
        {
            var chunks = streamChunks(playlistUrl);

            try
            {
                using (var outputStream = File.Create(videoTitle + ".ts"))
                {
                    using (WebClient client = new WebClient())
                    {
                        //client.Headers["Referer"] = video.PlayerUrl;
                        client.Headers["Accept"] = "*/*";

                        progress = new ProgressBar();

                        int totalTicks = chunks.Count;
                        int currentTick = 0;

                        foreach (var chunk in chunks)
                        {
                            progress.Report((double)currentTick / totalTicks);
                            using (MemoryStream stream = new MemoryStream(client.DownloadData(chunk)))
                            {
                                stream.CopyTo(outputStream);
                            }
                            currentTick++;
                        }
                    }
                }
            }
            catch (AmbiguousMatchException ex)
            {
            }
        }

        static List<string> streamChunks(string videoPlaylist)
        {
            List<string> tsChunks = new List<string>();

            using (var client = new WebClient())
            {
                client.Headers["Accept"] = "*/*";
                client.Headers["Referer"] = "https://www.playvids.com/";
                
                Stream playlist = client.OpenRead(videoPlaylist);
                StreamReader file = new StreamReader(playlist);

                string line;
                while ((line = file.ReadLine()) != null)
                {
                    if (line.Contains(".ts"))
                    {
                        tsChunks.Add(line);
                    }
                }
                file.Close();
            }
            return tsChunks;
        }

        private static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "");
        }
    }
}
