using System.Collections.Concurrent;
using System;
using SimpleJSON;

/** This is meant to be run to capture
  * networking events from a Client in a thread
  * you want to access it from.
  *
  * You basically instance it in your thread (or in main thread),
  * and point it at an instance of Client
  * then add your event listeners to it
  */
public class MessageDispatcher {
  public event MessageReceivedHandler messageReceivedEvent;
  public EventArgs messageEventArgs = null;
  public delegate void MessageReceivedHandler(MessageDispatcher dispatcher, EventArgs messageEvent);

  private Client client;
  public int maxDequeuePullSize = 16;

  public MessageDispatcher (Client client) {
    this.client = client;
  }

  public void pullEvents () {
    JSONNode message;
    int count = 0;
    OnMessageEventArgs evt = null;
    while (client.messageReceiveQueue.TryDequeue(out message)) {
      evt = new OnMessageEventArgs();
      evt.message = message;
      this.messageReceivedEvent(this, evt);
      count ++;
      if (count > maxDequeuePullSize) break;
    }
  }
}

public class OnMessageEventArgs : EventArgs
{
  public JSONNode message { get; set; }
}
