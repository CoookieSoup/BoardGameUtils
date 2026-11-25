
import { useEffect, useState } from 'react';
import toast from 'react-hot-toast';
import { useParams } from 'react-router-dom';
import './App.css';
import AsyncButton from './components/AsyncButton';
import Chat from './components/Chat';
import objectiveImage from './images/nemesisObjectiveTemplate.png';
import type { PlayerInfoResponse } from './requests/roomsApi';
import { loadObjectives, loadRoomPlayerInfo, revealObjectives, selectObjective, startGame } from './requests/roomsApi';
import { connection, connectToRoom, joinPlayerGroup } from "./signalr";
import './styling/objectiveCard.css';
import type { NemesisPlayerObjective } from './types/nemesisTypes';
import { ObjectiveType } from './types/nemesisTypes';

function Room() {
  const [players, setPlayers] = useState<PlayerInfoResponse[]>([]);
  const [objectives, setObjectives] = useState<NemesisPlayerObjective[]>([]);
  const [hasStarted, setHasStarted] = useState(false);
  const [isChoosing, setIsChoosing] = useState(false);
  const roomId = Number(sessionStorage.getItem("roomId"));
  const playerId = Number(sessionStorage.getItem("playerId"));
  const isLobbyLeader = sessionStorage.getItem("isLobbyLeader") === "true";
  const code = useParams().code || "";
  const password = sessionStorage.getItem("roomPassword");
  const hasChosen = objectives.length === 1;
  const isRevealed = players.some(player => player.id !== playerId && player.objective);

  useEffect(() => {
    if (!roomId) return;

    const refresh = async () => {
      const data = await loadRoomPlayerInfo(code);
      setPlayers(data);
    };

    const refreshObjectives = async () => {
      const data = await loadObjectives(code);
      setHasStarted(data.length > 0);
      setObjectives(data);
    };

    connection.on("PlayersChanged", refresh);
    connection.on("GameStarted", refreshObjectives);

    connectToRoom(roomId)
      .then(refresh)
      .then(refreshObjectives)
      .then(() => joinPlayerGroup());

    return () => {
      connection.off("PlayersChanged", refresh);
      connection.off("GameStarted", refreshObjectives);
    };
  }, [roomId]);

  return (
    <div className="page">
      <h1 className="title">{players.find(player => player.isLobbyLeader)?.nickname}'s Room</h1>
      <div className="room-layout">
      <div className="room-main">
      <div className="panel">
        {password && !hasStarted && (
          <p className="room-info">Room code: <b className="copyable" title="Click to copy" onClick={async () => {
            await navigator.clipboard.writeText(code);
            toast.success("Room code copied");
          }}>{code}</b> · Password: <b>{password}</b></p>
        )}
        <h2>Players</h2>
        <ul className="player-list">
          {players.map(player => (
            <li key={player.id}>
              <span>{player.nickname}</span>
              <span className="player-objective">{player.objective?.title}</span>
            </li>
          ))}
        </ul>
        {isLobbyLeader && !hasStarted && (
          <AsyncButton onClick={() => startGame(code)}>Start Game</AsyncButton>
        )}
        {isLobbyLeader && hasStarted && !isRevealed && (
          <AsyncButton onClick={() => revealObjectives(code)}>Reveal Objectives</AsyncButton>
        )}
      </div>
      {objectives.length > 0 && (
        <h2 className="section-title">{hasChosen ? "Your Objective" : "Choose Your Objective"}</h2>
      )}
      <div className="objectives-container">
        {objectives.map((objective, index) => (
            <div className={hasChosen ? "objective-card chosen" : "objective-card"} key={index}>
                <img src={objectiveImage} className="objective-frame" onClick={async () => {
                  if (hasChosen || isChoosing) return;
                  
                  setIsChoosing(true);
                  try {
                    await selectObjective(code, objective.id);
                    setObjectives([objective]);
                  } finally {
                    setIsChoosing(false);
                  }
                  }}/>
                <div className="objective-text">
                    <p>{objective.type === ObjectiveType.Cooperative ? "Cooperative" : "Solo"}</p>
                    <h2>{objective.title}</h2>
                </div>
            </div>
        ))}
      </div>
      </div>
      {roomId > 0 && <Chat code={code} playerId={playerId} players={players} />}
      </div>
    </div>
  );
}

export default Room;
