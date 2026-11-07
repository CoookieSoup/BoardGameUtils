import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import './App.css'
import TextInput from './components/inputs'
import { createRoom, joinRoom } from './requests/joinRoom'

function Home() {
  const [name, setName] = useState('')
  const [roomCode, setRoomCode] = useState('')
  const [maxPlayerCount, setMaxPlayerCount] = useState(0)
  const navigate = useNavigate()

  async function handleRoom(e: React.SubmitEvent<HTMLFormElement>, isCreate: boolean) {
    e.preventDefault();

    if (!name.trim() || (!isCreate && !roomCode.trim()) || (isCreate && !maxPlayerCount)) {
        return;
    }

    const data = isCreate ? await createRoom(name.trim(), maxPlayerCount) : await joinRoom(roomCode.trim().toUpperCase(), name.trim());

    sessionStorage.setItem("roomId", String(data.roomId));
    sessionStorage.setItem("playerId", String(data.playerId));
    sessionStorage.setItem("isLobbyLeader", String(isCreate));

    navigate(`/room/${data.roomCode}`);
  }

  return (
    <>
      <form onSubmit={(e) => handleRoom(e, false)}>
        <TextInput
          onChange={(value) => setName(value)}
          placeholder="Enter your name"
        />
        <TextInput
          onChange={(value) => setRoomCode(value)}
          placeholder="Enter room code"
        />
        <button type="submit">Join Room</button>
      </form>
      <form onSubmit={(e) => handleRoom(e, true)}>
        <TextInput
          onChange={(value) => setName(value)}
          placeholder="Enter your name"
        />
        <TextInput
          onChange={(value) => setMaxPlayerCount(Number(value))}
          placeholder="Enter max player count"
          type="number"
          validate={(value) => Number(value) > 1}
        />
        <button type="submit">Create Room</button>
      </form>
    </>
  )
}

export default Home