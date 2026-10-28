const API = "http://localhost:5018"; // use the port from dotnet run

export const joinRoom = async (code: string, name: string) => {
  const res = await fetch(`${API}/api/rooms/${code}/join`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ name }),
  }); 
  const data = await res.json();
  sessionStorage.setItem("player", JSON.stringify(data));

  return data;
};

export const createRoom = async (name: string, maxPlayerCount: number) => {
  const res = await fetch(`${API}/api/rooms/create`, {
    method: "POST",
    headers: { "Content-Type": "application/json" },
    body: JSON.stringify({ name, maxPlayerCount }),
  }); 
  const data = await res.json();

  return data;
};

export const loadRoomPlayerInfo = async (code: string) => {
  const res = await fetch(`${API}/api/rooms/${code}/getPlayerInfo`, {
    method: "GET",
    headers: { "Content-Type": "application/json" },
  }); 
  
  return await res.json();;
};