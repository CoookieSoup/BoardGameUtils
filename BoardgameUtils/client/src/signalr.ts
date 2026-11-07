import * as signalR from "@microsoft/signalr";

const API = "http://localhost:5018";

export const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${API}/hubs/lobby`)
  .withAutomaticReconnect()
  .build();

let started: Promise<void> | null = null;

export function connectToRoom(roomId: number) {
  if (!started) {
    started = connection.start();
  }
  return started.then(() => connection.invoke("Watch", roomId));
}

export function joinPlayerGroup(playerId: number, roomId: number) {
  if (!started) {
    started = connection.start();
  }
  return started.then(() => connection.invoke("JoinPlayerGroup", playerId, roomId));
}