using Newtonsoft.Json;
using System;
using System.IO;
using TerrariaInjector;

namespace TerraSocket
{
    public class TerraSocket
    {
        public static void Init()
        {
            GM.Logger.Info("TerraSocket Intiliazing");

            string ipPath = Path.Combine(Directory.GetCurrentDirectory(), "wsipconfig.json");
            ConfigModel config;
            if (File.Exists(ipPath))
            {
                string ipcontent = File.ReadAllText(ipPath);
                try
                {
                    config = JsonConvert.DeserializeObject<ConfigModel>(ipcontent);
                }
                catch (Exception e)
                {
                    GM.Logger.Error("Invalid content in wsipconfig.json", e);
                    config = DefaultIp();
                    File.WriteAllText(ipPath, JsonConvert.SerializeObject(config));//TODO: make it nice!
                }
            }
            else
            {
                config = DefaultIp();
                File.WriteAllText(ipPath, JsonConvert.SerializeObject(config));//TODO: make it nice!
            }
            Patches._server = new WebSocketServerHelper(config.Host, config.Port);

        }

        private static ConfigModel DefaultIp()
        {
            return new ConfigModel() { Host = "127.0.0.1", Port = 7394 };
        }
    }
}
