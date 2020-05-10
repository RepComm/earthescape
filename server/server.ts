
let encoder: TextEncoder = new TextEncoder();
let decoder: TextDecoder = new TextDecoder();

const tcpServer = Deno.listen({
  hostname: "localhost",
  port: 10209,
  transport: "tcp"
});

let msg = "Hello World";
let buff: Uint8Array;

class ClientConn {
  socket: Deno.Conn;
  receiveBuffer: Uint8Array;
  receiveBufferTemp: Uint8Array | undefined = undefined;
  receiveString: string = "";
  messageListeners: Array<Function> = new Array();
  readIntervalID: number = -1;
  readLock: boolean = false;
  constructor(socket: Deno.Conn) {
    this.socket = socket;
    this.receiveBuffer = new Uint8Array(1024);
  }

  listen(f: Function) {
    if (this.messageListeners.includes(f)) throw "Already included in listeners";
    this.messageListeners.push(f);
  }
  deafen(f: Function) {
    let ind = this.messageListeners.indexOf(f);
    if (ind !== -1) {
      this.messageListeners.splice(ind, 1);
    } else {
      throw "Listener not in listeners";
    }
  }

  sendBytes(bytes: Uint8Array) {
    this.socket.write(bytes);
  }

  sendString(msg: string) {
    this.sendBytes(encoder.encode(msg));
  }

  sendJson(msg: object) {
    this.sendString(JSON.stringify(msg));
  }

  close() {
    console.log("Closing socket at EOF");
    this.socket.close();
  }

  beginReadLoop() {
    this.readIntervalID = setInterval(() => {
      if (!this.readLock) {
        this.readLock = true;
        this.socket.read(this.receiveBuffer).then((num) => {
          if (num == Deno.EOF) { this.close(); return; }
          this.receiveBufferTemp = this.receiveBuffer.subarray(0, num);
          this.receiveString = decoder.decode(this.receiveBufferTemp);
          for (let listener of this.messageListeners) {
            listener(this, this.receiveString);
          }
          this.readLock = false;
        });
      }
    }, 10);
  }

  endReadLoop() {
    clearInterval(this.readIntervalID);
  }
}

let clients = new Array<ClientConn>();

function onClientMessage(client: ClientConn, msg: string) {
  //console.log(`[Server] Client ${client} sent ${msg}`);
  try {
    console.log(JSON.parse(msg));
  } catch (ex) {
    console.log("\"" + msg + "\"");
  }
  client.sendString(JSON.stringify({type:"test",msg:"Hello World"}));
}

for await (const socket of tcpServer) {
  let client = new ClientConn(socket);
  clients.push(client);
  client.listen(onClientMessage);
  console.log("Client connected");
  client.beginReadLoop();
}
