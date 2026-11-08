
import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { useParams } from 'react-router-dom';
import './App.css';
import objectiveImage from './images/nemesisObjectiveTemplate.png';
import type { PlayerInfoResponse } from './requests/roomsApi';
import { loadRoomPlayerInfo, selectObjective } from './requests/roomsApi';
import { connection, connectToRoom, joinPlayerGroup } from "./signalr";
import './styling/objectiveCard.css';
import type { NemesisPlayerObjective } from './types/nemesisTypes';
import { ObjectiveType } from './types/nemesisTypes';

function Room() {
  const [players, setPlayers] = useState<PlayerInfoResponse[]>([]);
  const [objectives, setObjectives] = useState<NemesisPlayerObjective[]>([]);
  const [hasStarted, setHasStarted] = useState(false);
  const roomId = Number(sessionStorage.getItem("roomId"));
  const playerId = Number(sessionStorage.getItem("playerId"));
  const isLobbyLeader = sessionStorage.getItem("isLobbyLeader") === "true";
  const code = useParams().code || "";

  useEffect(() => {
    if (!roomId) return;

    const refresh = async () => {
      const data = await loadRoomPlayerInfo(code);
      setPlayers(data);
    };

    connection.on("PlayersChanged", refresh);

    connection.on("ObjectivesReceived", (data) => {
      setHasStarted(true);
      setObjectives(data);
    });

    connectToRoom(roomId)
      .then(refresh)
      .then(() => joinPlayerGroup(playerId, roomId));

    return () => {
      connection.off("PlayersChanged", refresh);
      connection.off("ObjectivesReceived");
    };
  }, [roomId]);

  const startGame = async () => {
    try {
      await connection.invoke("StartGame", roomId);
    } catch (error) {
      const serverMessage = String(error).split("HubException: ")[1];
      toast.error(serverMessage ?? "Could not start the game");
    }
  };

  return (
    <div>
      {players.map(player => (
        <div>{player.nickname} {player.objective?.title}</div>
      ))}
      {isLobbyLeader && !hasStarted && (
        <button onClick={startGame}>Start Game</button>
      )}
      <div className="objectives-container">
        {objectives.map((objective, index) => (
            <div className="objective-card" key={index}>
                <img src={objectiveImage} className="objective-frame" onClick={async () => {
                  console.log(`Player ${playerId} selected objective ${objective.title} in room ${roomId}`);
                  await selectObjective(code, playerId, objective.id);
                  }}/>
                <div className="objective-text">
                    <p>{objective.type === ObjectiveType.Cooperative ? "Cooperative" : "Solo"}</p>
                    <h2>{objective.title}</h2>
                </div>
            </div>
        ))}
      </div>
    </div>
  );
}

export default Room;
