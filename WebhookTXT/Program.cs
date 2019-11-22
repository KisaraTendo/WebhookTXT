using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using System.IO;
using Newtonsoft.Json;
using System.Threading;
using Newtonsoft.Json.Linq;

namespace WebhookTXT
{
    class Program
    {
        private static readonly HttpClient client = new HttpClient();
        public static string WebhookURL { get; set; }


        public class ContentData
        {
            [JsonProperty("content")]
            public string content { get; set; }
        }

        public static void sendRequest(string PayloadData)
        {
            var payload = new ContentData
            {
                content = PayloadData
            };
            var StringPayload = JsonConvert.SerializeObject(payload);
            var HttpContent = new StringContent(StringPayload, Encoding.UTF8, "application/json");
            client.PostAsync(WebhookURL, HttpContent);
        }


        static void Start(string textFile)
        {
            string json = File.ReadAllText(@"settings.json").ToString();
            JObject jObject = JObject.Parse(json);

            WebhookURL = jObject["WebhookURL"].ToString();
            Console.WriteLine($"Loaded: {WebhookURL} from settings.json"); 

            if (!string.IsNullOrWhiteSpace(WebhookURL) && WebhookURL.Contains("https://discordapp.com/api/webhooks/"))
            {
                string PayloadData = string.Empty;
                using (FileStream fs = File.Open(textFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    using (BufferedStream bs = new BufferedStream(fs))
                    {
                        using (StreamReader sr = new StreamReader(bs))
                        {
                            string fileLine = string.Empty;
                            while ((fileLine = sr.ReadLine()) != null)
                            {
                                if (PayloadData.Length < 2000)
                                {
                                    PayloadData += fileLine + "\n";
                                }
                                else
                                {
                                    sendRequest(PayloadData);
                                    PayloadData = string.Empty;
                                    Thread.Sleep(4000);

                                }
                            }
                        }
                    }
                }
                sendRequest(PayloadData);
            }
            else
            {
                Console.WriteLine("Not a valid Discord Webhook URL! Press any key to exit!");
                Console.ReadLine();
                return;
            }
        }

        static void Main(string[] args)
        {
            if (args.Length >= 1 && File.Exists(args[0]))
            {
                Start(args[0]);
                Console.WriteLine("\n\nTask has been completed! Messages may take some time to be sent. Press any key to exit...");
                Console.ReadLine();
            }
            else
            {
                Console.WriteLine("No arguments have been provided or file does not exist at path!");
                Console.ReadLine();
                return;

            }
            
        }
    }
}
