﻿using Newtonsoft.Json;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace TerraSocket
{
    public class WebSocketServerHelper
    {
        public WebSocketServerHelper(string ip = "ws://127.0.0.1", short port = 7394)
        {
            wssv = InitializeServer(ip, port);
        }

        public static WebSocketServer wssv { get; set; }
        public void CloseServer()
        {
            wssv.Stop();
        }
        public WebSocketServer InitializeServer(string ip = "ws://127.0.0.1", short port = 7394)
        {
            wssv = new WebSocketServer(ip + ':' + port);
            wssv.AddWebSocketService<Startup>("/");
            wssv.Start();
            Logger.Info($"WebSocket server started at \"{ip + ':' + port}\"");
            return wssv;
        }
        public void SendWSMessage(WebSocketMessageModel msg)
        {
            string jsonMessage = JsonConvert.SerializeObject(msg);
            if (!(wssv is null))
            {
                wssv.WebSocketServices.Broadcast(jsonMessage);
                Logger.Info($"\"{msg.Event}\" sent to clients.");
            }
            else
            {
                Logger.Warn("WebSocket Server not found.");
            }

        }
    }
    public class Startup : WebSocketBehavior
    {
        protected override void OnClose(CloseEventArgs e)
        {
            Logger.Info($"Client Disconnected. ID:{ID}");
            base.OnClose(e);
        }
        protected override void OnOpen()
        {
            Logger.Info($"Client joined. ID:{ID}");
            base.OnOpen();
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            Logger.Info($"Message received: {e.Data}");
            Commands.CommandHandler(e.Data);
            base.OnMessage(e);
        }
        protected override void OnError(ErrorEventArgs e)
        {
            Logger.Error("WebSocket Error", e.Exception);
            base.OnError(e);
        }
    }
}