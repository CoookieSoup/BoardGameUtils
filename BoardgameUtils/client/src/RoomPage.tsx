
import { useEffect, useState } from 'react';
import { useParams } from 'react-router-dom';
import './App.css';
import objectiveImage from './images/nemesisObjectiveTemplate.png';
import type { PlayerInfoResponse } from './requests/roomsApi';
import { loadObjectives, loadRoomPlayerInfo, selectObjective, startGame } from './requests/roomsApi';
import { connection, connectToRoom, joinPlayerGroup } from "./signalr";
import './styling/objectiveCard.css';
import type { NemesisPlayerObjective } from './types/nemesisTypes';
import { ObjectiveType } from './types/nemesisTypes';

function Room() {
  const [players, setPlayers] = useState<PlayerInfoResponse[]>([]);
  const [objectives, setObjectives] = useState<NemesisPlayerObjective[]>([]);
  const [hasStarted, setHasStarted] = useState(false);
  const roomId = Number(sessionStorage.getItem("roomId"));
  const isLobbyLeader = sessionStorage.getItem("isLobbyLeader") === "true";
  const code = useParams().code || "";
  const password = sessionStorage.getItem("roomPassword");
  const hasChosen = objectives.length === 1;

  useEffect(() => {
    if (!roomId) return;

    const refresh = async () => {
      const data = await loadRoomPlayerInfo(code);
      setPlayers(data);
    };

    const refreshObjectives = async () => {
      const data = await loadObjectives(code);
      setHasStarted(true);
      setObjectives(data);
    };

    connection.on("PlayersChanged", refresh);
    connection.on("GameStarted", refreshObjectives);

    connectToRoom(roomId)
      .then(refresh)
      .then(() => joinPlayerGroup());

    return () => {
      connection.off("PlayersChanged", refresh);
      connection.off("GameStarted", refreshObjectives);
    };
  }, [roomId]);

  return (
    <div>
      {password && !hasStarted && (
        <p>Room code: <b>{code}</b> · Password: <b>{password}</b></p>
      )}
      {players.map(player => (
        <div>{player.nickname} {player.objective?.title}</div>
      ))}
      {isLobbyLeader && !hasStarted && (
        <button onClick={() => startGame(code)}>Start Game</button>
      )}
      <div className="objectives-container">
        {objectives.map((objective, index) => (
            <div className={hasChosen ? "objective-card chosen" : "objective-card"} key={index}>
                <img src={objectiveImage} className="objective-frame" onClick={async () => {
                  if (hasChosen) return;
                  await selectObjective(code, objective.id);
                  setObjectives([objective]);
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
