import * as signalR from "@microsoft/signalr";

const API = "http://localhost:5018";

export const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${API}/hubs/lobby`)
  .withAutomaticReconnect()
  .build();

let started: Promise<void> | null = null;

export function connectToRoom(code: string) {
  if (!started) {
    started = connection.start();
  }
  return started.then(() => connection.invoke("Watch", code));
}