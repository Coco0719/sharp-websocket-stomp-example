using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using WebSocketSharp;

namespace ControlTestApp
{
    class Program
    {
        static public void onOpenAction(WebSocket ws, StompMessageSerializer serializer)
        {
            Console.WriteLine("Server says : open");
            var connect = new StompMessage("CONNECT");
            connect["accept-version"] = "1.1";
            connect["heart-beat"] = "10000,10000";
            ws.Send(serializer.Serialize(connect));
        }
        static public void onMessageAction(WebSocket ws, StompMessageSerializer serializer, string data, int clientId)
        {
            StompMessage msg = serializer.Deserialize(data);
            if (msg.Command == StompCommand.CONNECTED)
            {
                var sub = new StompMessage("SUBSCRIBE");
                sub["id"] = "sub-" + clientId;
                sub["destination"] = "/topic/" + clientId;
                ws.Send(serializer.Serialize(sub));
            }
            else if (msg.Command == StompCommand.MESSAGE)
            {
                JObject jObj = JObject.Parse(msg.Body);
                string rMsg = (string)jObj["msg"];
                Console.WriteLine(rMsg);
            }
        }

        static async Task Main(string[] args)
        {
            StompMessageSerializer serializer = new StompMessageSerializer();

            int clientId = 1;

            var apiWs = new WebSocket("ws://localhost:8083/api-ws");
            apiWs.OnOpen += (sender, e) => onOpenAction(apiWs, serializer);
            apiWs.OnMessage += (sender, e) => onMessageAction(apiWs, serializer, e.Data, clientId);
            apiWs.Connect();
            Console.WriteLine("api connection open");

            //var mainWs = new WebSocket("ws://localhost:8084/main-ws");
            //mainWs.OnOpen += (sender, e) => onOpenAction(mainWs, serializer);
            //mainWs.OnMessage += (sender, e) => onMessageAction(mainWs, serializer, e.Data, clientId);
            //mainWs.Connect();
            //Console.WriteLine("main connection open");

            while (true)
            {
                ConsoleKeyInfo key = Console.ReadKey(true);
                if(key.KeyChar == 'e')
                {
                    apiWs.Close();
                    Console.WriteLine("api connection close ");
                    //mainWs.Close();
                    //Console.WriteLine("main connection close");
                    break;
                }

                Dictionary<object, object> payload = new Dictionary<object, object>
                {
                    {  "tId", (long) (DateTime.UtcNow - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds },
                    {  "location", new Dictionary<object, object> {
                                            {"farmerId", 1},
                                            {"houseId", 5},
                                            {"farmId", null}
                    }},
                    {  "paramsDetail", new Dictionary<object, object> {
                                            {"totalAmount", 500},
                                            {"aNutrientRatio", 5},
                                            {"bNutrientRatio", 2},
                                            {"waterRatio", 3}
                    }}
                };

                HttpClient client = new HttpClient();
                client.DefaultRequestHeaders
                    .Add("Authorization", " Bearer " + "eyJhbGciOiJIUzI1NiJ9.eyJlbWFpbCI6ImVqc3ZrMzI4NEBuYXZlci5jb20ifQ.SySydZopWvXj8DONpTJuqErP28C-JfsERI086avGKrk");

                StringContent content = new StringContent(
                    JsonConvert.SerializeObject(payload, Formatting.Indented),
                    Encoding.UTF8,
                    "application/json");

                client.BaseAddress = new Uri("http://192.168.0.4:8083");
                HttpResponseMessage response = await client.PostAsync("/api/auth/hus/C_M_005", content);

                if (response.StatusCode == System.Net.HttpStatusCode.Created)
                {
                    Console.WriteLine("제어 완료");
                    Console.WriteLine(response.ToString());
                }
                else
                {
                    Console.WriteLine("제어 실패");
                }
            }
        }
    }
}
