﻿//*********************************************************************
// Description:
// Author: hiramtan@live.com
//*********************************************************************

using System;
using System.Net.Sockets;
using UnityEngine;

namespace HiSocket
{
    public class TcpSocket : ISocket, IDisposable
    {
        private int bufferSize = 1024;
        private string ip;
        private int port;
        public byte[] buffer;
        private TcpClient client;
        private int timeOut = 5000;//5s
        public bool IsConnected { get { return client.Client != null && client.Connected; } }

        public TcpSocket()
        {
            client = new TcpClient();
            client.NoDelay = true;
            client.SendTimeout = client.ReceiveTimeout = timeOut;
            buffer = new byte[bufferSize];
        }

        public void Connect(string paramIp, int paramPort, Action paramEventHandler = null)
        {
            ip = paramIp;
            port = paramPort;
            try
            {
                client.BeginConnect(ip, port, new AsyncCallback(delegate (IAsyncResult ar)
                {
                    try
                    {
                        TcpClient tempTcpClient = (TcpClient)ar.AsyncState;
                        tempTcpClient.EndConnect(ar);
                        if (paramEventHandler != null)
                            paramEventHandler();
                        client.Client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(Receive), client);
                    }
                    catch (Exception e)
                    {
                        Debug.LogError(e.ToString());
                    }
                }), client);
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }
        }

        public void Send(byte[] param)
        {
            client.Client.BeginSend(param, 0, param.Length, SocketFlags.None, new AsyncCallback(delegate (IAsyncResult ar)
                 {
                     try
                     {
                         TcpClient tempTcpClient = (TcpClient)ar.AsyncState;
                         tempTcpClient.Client.EndSend(ar);
                     }
                     catch (Exception e)
                     {
                         Debug.LogError(e.ToString());
                     }
                 }), client);
        }

        public void Receive(IAsyncResult ar)
        {
            try
            {
                TcpClient tempTcpClient = (TcpClient)ar.AsyncState;
                int temp = tempTcpClient.Client.EndReceive(ar);
                if (temp > 0)
                {
                    //handle buffer
                    Array.Clear(buffer, 0, buffer.Length);
                    client.Client.BeginReceive(buffer, 0, buffer.Length, SocketFlags.None, new AsyncCallback(Receive), client);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(e.ToString());
            }

        }

        public void Close()
        {
            if (IsConnected)
            {
                client.Close();
                client = null;
            }
        }

        public void Dispose()
        {
            //throw new NotImplementedException();
        }
    }
}