﻿#if !NETSTANDARD
using Bittrex.Net.Interfaces;
using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Security.Authentication;
using WebSocketSharp;

namespace Bittrex.Net.Sockets
{
    public class WebsocketSharp : IWebsocket
    {
        List<Action<Exception>> errorhandlers = new List<Action<Exception>>();
        List<Action> openhandlers = new List<Action>();
        List<Action> closehandlers = new List<Action>();
        List<Action<string>> messagehandlers = new List<Action<string>>();
        WebSocket socket;

        public WebsocketSharp(string url, string cookieHeader, string userAgent)
        {
            socket = new WebSocket(url);

            socket.SslConfiguration.EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls11 | SslProtocols.Tls;
            socket.CustomHeaders = new Dictionary<string, string>()
            {
                { "Cookie", cookieHeader },
                { "User-Agent", userAgent },
            };

            socket.OnError += HandleError;
            socket.OnOpen += HandleOpen;
            socket.OnClose += HandleClose;
            socket.OnMessage += HandleMessage;
        }

        private void HandleError(object sender, ErrorEventArgs e)
        {
            foreach (var handler in errorhandlers)
                handler(e.Exception);
        }

        private void HandleOpen(object sender, EventArgs e)
        {
            foreach (var handler in openhandlers)
                handler();
        }

        private void HandleClose(object sender, CloseEventArgs e)
        {
            // List recreation as workaround so that collection does not get modified on foreach
            foreach (Action handler in new List<Action>(closehandlers))
                handler();
        }

        private void HandleMessage(object sender, MessageEventArgs e)
        {
            foreach (var handler in messagehandlers)
                handler(e.Data);
        }

        public event Action<Exception> OnError
        {
            add { errorhandlers.Add(value); }
            remove { errorhandlers.Remove(value); }
        }
        public event Action OnOpen
        {
            add { openhandlers.Add(value); }
            remove { openhandlers.Remove(value); }
        }
        public event Action OnClose
        {
            add { closehandlers.Add(value); }
            remove { closehandlers.Remove(value); }
        }
        public event Action<string> OnMessage
        {
            add { messagehandlers.Add(value); }
            remove { messagehandlers.Remove(value); }
        }
        
        public void Close()
        {
            socket.Close();
        }

        public void setProxy(IWebProxy connectionProxy)
        {
            Uri proxy;
            try
            {
                proxy = connectionProxy.GetProxy(socket.Url);
            }
            catch (NullReferenceException)
            {
                // no proxy is set, we can skip this
                return;
            }

            string host = proxy.Host;
            int proxyPort = proxy.Port;

            if (host != "" && proxyPort != 0)
            {
                socket.SetProxy(String.Format("http://{0}:{1}", host, proxyPort) , null, null);
            }
        }

        public bool IsClosed()
        {
            return socket.ReadyState == WebSocketState.Closed;
        }

        public bool IsOpen()
        {
            return socket.ReadyState == WebSocketState.Open;
        }

        public void Open()
        {
            socket.Connect();
        }

        public void Send(string data)
        {
            socket.Send(data);
        }
    }
}
#endif