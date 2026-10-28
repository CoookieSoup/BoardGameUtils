import { useState } from 'react'
import { useNavigate } from 'react-router-dom'
import './App.css'
import { createRoom, joinRoom } from './requests/joinRoom'

function Home() {
  const [name, setName] = useState('')
  const [roomCode, setRoomCode] = useState('')
  const [maxPlayerCount, setMaxPlayerCount] = useState(0)
  const navigate = useNavigate()

  async function handleJoin(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault()

    if (!name.trim() || !roomCode.trim()) {
      return
    }

    await joinRoom(roomCode.trim().toUpperCase(), name.trim())
    navigate(`/room/${roomCode.trim().toUpperCase()}`);
  }

  async function handleCreate(e: React.SubmitEvent<HTMLFormElement>) {
    e.preventDefault()

    if (!name.trim()) {
      return
    }

    const data = await createRoom(name.trim(), maxPlayerCount);
    navigate(`/room/${data.roomCode.trim().toUpperCase()}`);
  }

  return (
    <>
      <form onSubmit={handleJoin}>
        <input
          type="text"
          onChange={(e) => setName(e.target.value)}
          placeholder="Enter your name"
          required
        />

        <input
          type="text"
          value={roomCode}
          onChange={(e) => setRoomCode(e.target.value)}
          placeholder="Enter room code"
          required
        />

        <button type="submit">Join Room</button>
      </form>
      <form onSubmit={handleCreate}>
        <input
          type="text"
          onChange={(e) => setName(e.target.value)}
          placeholder="Enter your name"
          required
        />
        <input
          type="number"
          onChange={(e) => setMaxPlayerCount(Number(e.target.value))}
          placeholder="Enter max player count"
          required
        />
        <button type="submit">Create Room</button>
      </form>
    </>
  )
}

export default Home