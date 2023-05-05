using Newtonsoft.Json;
using TerrariaInjector;
using WebSocketSharp;
using WebSocketSharp.Server;

namespace TerraSocket
{
    public class WebSocketServerHelper
    {
        public WebSocketServerHelper(string ip = "127.0.0.1", ushort port = 7394)
        {
            wssv = InitializeServer(ip, port);
        }

        public static WebSocketServer wssv { get; set; }
        internal static WebSocketSessionManager SessionManager;
        public void CloseServer()
        {
            wssv.Stop();
        }
        private WebSocketServer InitializeServer(string ip, ushort port)
        {
            string addr = string.Format("ws://{0}:{1}",ip,port); 
            wssv = new WebSocketServer(addr);
            wssv.AddWebSocketService<Startup>("/");
            wssv.Start();
            GM.Logger.Info(string.Format("WebSocket server started at \"{0}:{1}\"",ip,port));
            return wssv;
        }
        public void SendWSMessage(WebSocketMessageModel msg)
        {
            string jsonMessage = JsonConvert.SerializeObject(msg);
            if (!(wssv is null))
            {
                SessionManager.Broadcast(jsonMessage);
                GM.Logger.Info(string.Format("\"{0}\" sent to clients.",msg.Event));
            }
            else
            {
                GM.Logger.Warning("WebSocket Server not found.");
            }

        }
    }
    public class Startup : WebSocketBehavior
    {
        protected override void OnClose(CloseEventArgs e)
        {
            GM.Logger.Info(string.Format("Client Disconnected. ID:{0}",ID));
            base.OnClose(e);
        }
        protected override void OnOpen()
        {
            GM.Logger.Info(string.Format("Client joined. ID:{0}",ID));
            WebSocketServerHelper.SessionManager = Sessions;
            base.OnOpen();
        }
        protected override void OnMessage(MessageEventArgs e)
        {
            GM.Logger.Info(string.Format("Message received: {0}",e.Data));
            Commands.CommandHandler(e.Data);
            base.OnMessage(e);
        }
        protected override void OnError(ErrorEventArgs e)
        {
            GM.Logger.Error("WebSocket Error", e.Exception);
            base.OnError(e);
        }
    }
}
