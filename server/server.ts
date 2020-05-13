
import { writeFileStrSync, readJsonSync } from "https://deno.land/std/fs/mod.ts";

let resources: any = {};

function saveResources () {
  writeFileStrSync(
    "./resources.json",
    JSON.stringify(resources, (key, value)=>{
      if (key === "instance") return undefined;
      return value;
    })
  );
}

function loadResources () {
  let rez: any = readJsonSync("./resources.json");
  for (let key in rez) {
    //Skip unconfirmed clients
    if (rez[key].type === "client" && rez[key].confirmed === false) continue;
    resources[key] = rez[key];
  }
}

loadResources();

function createResourceId (): string {
  let highest = 0;
  let ikey:number;
  for (let key in resources) {
    ikey = parseInt(key);
    if (ikey > highest) highest = ikey;
  }
  return (highest + 1).toString();
}

let encoder: TextEncoder = new TextEncoder();
let decoder: TextDecoder = new TextDecoder();

const tcpServer = Deno.listen({
  hostname: "localhost",
  port: 10209,
  transport: "tcp"
});

class ClientConn {
  resourceId: string = "-1";
  socket: Deno.Conn;
  receiveBuffer: Uint8Array;
  receiveBufferTemp: Uint8Array | undefined = undefined;
  receiveString: string = "";
  messageListeners: Array<Function> = new Array();
  readIntervalID: number = -1;
  readLock: boolean = false;
  constructor(socket: Deno.Conn, resourceId: string) {
    this.socket = socket;
    this.receiveBuffer = new Uint8Array(1024);
    this.resourceId = resourceId;
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

function broadcast (msg: string, except:ClientConn|undefined) {
  for (let client of clients) {
    if (except === undefined || client !== except) {
      client.sendString(msg);
    }
  }
}

const broadcastMessageTypes = [
  "init",
  "chat",
  "move",
  "setblock"
];

function shouldBroadcast (json: {type?:string}): boolean {
  let type: string|undefined = json["type"];
  return type===undefined||broadcastMessageTypes.includes(type);
}

function onClientMessage(client: ClientConn, msg: string) {
  let json;
  try {
    json = JSON.parse(msg);
  } catch (ex) {
    console.log("JSON wasn't parsed correctly!", msg);
    return;
  }
  console.log(json);
  switch (json.type) {
    case "init":
      resources[json.oldResourceId] = undefined;
      delete resources[json.oldResourceId];
      resources[json.resourceId] = {
        instance:client,
        address:client.socket.remoteAddr,
        confirmed:true
      };
      client.resourceId = json.resourceId;
      saveResources();
      break;
    case "create-resource-id":
      let packet = {
        type:"respond-resource-id",
        resourceId:createResourceId()
      }
      resources[packet.resourceId] = {
        owner:client.resourceId,
        resourceId:packet.resourceId,
        type:"pending"
      };
      client.sendJson(packet);
      break;
    case "create-ship":
      if (resources[json.resourceId] === undefined) throw "Please patch me..";
      let rez = resources[json.resourceId];
      if (rez.type === "pending") {
        rez.type = "ship";
        rez.data = json.data;
      }
      break;
  }
  if (shouldBroadcast(json)) {
    broadcast(msg, client);
  }
}

for await (const socket of tcpServer) {
  let client = new ClientConn(socket, createResourceId());
  resources[client.resourceId] = {
    instance:client,
    type:"client",
    address:socket.remoteAddr,
    confirmed:false
  };
  saveResources();

  clients.push(client);
  client.listen(onClientMessage);
  console.log("Client connected ", socket.remoteAddr);
  client.beginReadLoop();
  client.sendJson({
    type:"init",
    resourceId:client.resourceId
  });
}
