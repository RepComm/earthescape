using System.Collections;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Net.Sockets;
using System.Threading;
using System.Text;
using System;
using System.IO;
using UnityEngine;
using SimpleJSON;

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
  JSONObject serverConfig = null;

  public ConcurrentQueue<JSONNode> messageReceiveQueue = new ConcurrentQueue<JSONNode>();

  public Client()
  {
    loadServerConfigFile();
    if (Client.instance != null) throw new Exception("Multiple clients not implemented yet");
    Client.instance = this;
  }

  private void saveServerConfigFile()
  {
    using (StreamWriter jsonFileWriter = new StreamWriter(
      Path.Combine(
        Application.persistentDataPath,
        "config.json"
      )
    ))
    {
      if (this.serverConfig == null)
      {
        this.serverConfig = new JSONObject();
        this.serverConfig.Add("resourceId", new JSONString("-1"));
      }
      jsonFileWriter.Write(this.serverConfig.ToString());
    }
  }

  private void loadServerConfigFile()
  {
    if (!File.Exists(Path.Combine(
      Application.persistentDataPath,
      "config.json"
    )))
    {
      saveServerConfigFile();
    }
    string text = System.IO.File.ReadAllText(Path.Combine(
      Application.persistentDataPath,
      "config.json"
    ));
    this.serverConfig = JSON.Parse(text).AsObject;
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
    JSONObject connectMsg = new JSONObject();
    connectMsg.Add("type", new JSONString("connect"));
    connectMsg.Add("host", new JSONString(this.clientHost));
    connectMsg.Add("port", new JSONNumber(this.clientPort));
    messageReceiveQueue.Enqueue(connectMsg);

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
        Debug.Log(msg);
        JSONNode json = JSON.Parse(msg);
        this.handleInternalMessage(json);
        this.messageReceiveQueue.Enqueue(json);
      }
    }
  }

  private void handleInternalMessage(JSONNode json)
  {
    JSONObject msg = json.AsObject;
    Debug.Log("Handling internally:" + msg.ToString());
    if (msg["type"] == "init")
    {
      JSONObject initMsg = new JSONObject();

      if (!this.serverConfig.HasKey("resourceId"))
      {
        this.serverConfig.Add("resourceId", new JSONString(msg["resourceId"]));
      }

      initMsg.Add("resourceId", new JSONString(this.serverConfig["resourceId"]));
      this.SendJSON(initMsg);
    }
  }

  public void Disconnect()
  {
    this.clientKeepAlive = false;
    this.clientSocketThread.Abort();
  }

  public bool SendJSON(JSONNode msg)
  {
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
