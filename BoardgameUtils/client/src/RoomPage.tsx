import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import './App.css';
import { loadRoomPlayerInfo } from './requests/joinRoom';
import { connection, connectToRoom } from "./signalr";

function Room() {
  const [players, setPlayers] = useState<{ id: number; nickname: string }[]>([]);
  const { currentLobbyCode } = useParams();

  const refresh = async () => {
  const data = await loadRoomPlayerInfo(currentLobbyCode!);
    setPlayers(data);
  };

  useEffect(() => {
    connection.on("RoomChanged", refresh);
    connectToRoom(currentLobbyCode!).then(refresh);

    return () => {
      connection.off("RoomChanged", refresh);
    };
  }, []);

  return (
    <>
        <input type="text" placeholder="Enter your name" />
        <p> {players.length}</p>
    </>
     
  )
}

export default Room
