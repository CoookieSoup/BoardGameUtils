import * as signalR from "@microsoft/signalr";

const API = "http://localhost:5018";

export const connection = new signalR.HubConnectionBuilder()
  .withUrl(`${API}/hubs/lobby`, {
    accessTokenFactory: () => sessionStorage.getItem("token") ?? "",
  })
  .withAutomaticReconnect()
  .build();

let started: Promise<void> | null = null;

export function connectToRoom(roomId: number) {
  if (!started) {
    started = connection.start();
  }
  return started.then(() => connection.invoke("Watch", roomId));
}

export function joinPlayerGroup() {
  if (!started) {
    started = connection.start();
  }
  return started.then(() => connection.invoke("JoinPlayerGroup"));
}