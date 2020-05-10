using System.Collections;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System;
using SimpleJSON;
using UnityEngine;

public class Client
{
  public static Client instance;
  Thread clientSocketThread;
  TcpClient clientSocket;
  NetworkStream clientStream;
  string clientHost = "localhost";
  int clientPort = 10209;
  byte[] clientReceiveBuffer = new byte[1024];
  byte[] clientSendBuffer;
  int clientReceiveBufferLength = 0;
  bool clientKeepAlive = false;

  public event MessageReceivedHandler messageReceivedEvent;
  public EventArgs messageEventArgs = null;
  public delegate void MessageReceivedHandler(Client client, EventArgs messageEvent);

  public event ConnectHandler connectEvent;
  public EventArgs connectEventArgs = null;
  public delegate void ConnectHandler(Client client, EventArgs connectEventArgs);

  public Client () {
    if (Client.instance != null) throw new Exception("Multiple clients not implemented yet");
    Client.instance = this;
  }

  public void Connect(string host, int port)
  {
    this.clientHost = host;
    this.clientPort = port;
    this.clientSocketThread = new Thread(new ThreadStart(this.ConnectListener));
    this.clientSocketThread.IsBackground = true;
    this.clientSocketThread.Start();
  }

  private void ConnectListener()
  {
    this.clientSocket = new TcpClient(this.clientHost, this.clientPort);
    this.clientStream = this.clientSocket.GetStream();
    this.clientKeepAlive = true;
    while (!this.clientSocket.Connected)
    {
      //Add timeout here
    }
    OnConnectEventArgs oca = new OnConnectEventArgs();
    oca.host = this.clientHost;
    if (this.connectEvent != null)
    {
      this.connectEvent(this, oca);
    }
    while (this.clientKeepAlive)
    {
      while (
        (this.clientReceiveBufferLength = this.clientStream.Read(
          this.clientReceiveBuffer,
          0,
          this.clientReceiveBuffer.Length
        )
      ) != 0)
      {
        string msg = Encoding.ASCII.GetString(this.clientReceiveBuffer, 0, this.clientReceiveBufferLength);
        OnMessageEventArgs oma = new OnMessageEventArgs();
        oma.message = msg;
        if (this.messageReceivedEvent != null)
        {
          this.messageReceivedEvent(this, oma);
        }
      }
    }
  }

  public void Disconnect()
  {
    this.clientKeepAlive = false;
    this.clientSocketThread.Abort();
  }

  public bool SendJSON (JSONNode msg) {
    this.SendMessage(msg.ToString());
    return true;
  }

  public bool SendMessage(string msg)
  {
    if (this.clientStream == null)
    {
      Debug.Log("Couldn't get stream!");
      return false;
    }
    try
    {
      if (this.clientStream.CanWrite)
      {
        this.clientSendBuffer = Encoding.ASCII.GetBytes(msg);
        this.clientStream.Write(
          this.clientSendBuffer,
          0,
          this.clientSendBuffer.Length
        );
        return true;
      }
      else
      {
        return false;
      }
    }
    catch (SocketException ex)
    {
      Debug.Log(ex);
      return false;
    }
  }
}

public class OnMessageEventArgs : EventArgs
{
  public string message { get; set; }
}

public class OnConnectEventArgs : EventArgs
{
  public string host { get; set; }
}

