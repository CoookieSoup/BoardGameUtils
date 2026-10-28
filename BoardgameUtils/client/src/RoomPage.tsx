import { useCallback, useEffect, useRef, useState } from 'react';
import { useParams } from 'react-router-dom';
import './App.css';
import { loadRoomPlayerInfo } from './requests/joinRoom';
import { connection, connectToRoom } from "./signalr";

function Room() {
  const [players, setPlayers] = useState<{ id: number; nickname: string }[]>([]);
  const latestRequest = useRef(0);
  const { code  } = useParams();

  const refresh = useCallback(async () => {
    if (!code) return;

    const request = ++latestRequest.current;
    const data = await loadRoomPlayerInfo(code);
    if (request === latestRequest.current) {
      setPlayers(data);
    }
  }, [code]);

  useEffect(() => {
    connection.on("PlayersChanged", refresh);
    connectToRoom(code!).then(refresh);

    return () => {
      latestRequest.current++;
      connection.off("PlayersChanged", refresh);
    };
  }, [code, refresh]);

  return (
    <>
        <input type="text" placeholder="Enter your name" />
        {players.map(player => (
          <li key={player.id}>{player.nickname}</li>
        ))}
    </>
     
  )
}

export default Room
