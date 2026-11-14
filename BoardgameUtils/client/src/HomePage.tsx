import { useNavigate } from 'react-router-dom'
import './App.css'
import TextInput from './components/inputs'
import { createRoom, joinRoom } from './requests/roomsApi'

function Home() {
  const navigate = useNavigate()

  async function handleRoom(e: React.SubmitEvent<HTMLFormElement>, isCreate: boolean) {
    e.preventDefault();

    const form = new FormData(e.currentTarget);
    const name = String(form.get("name") ?? "").trim();
    const password = String(form.get("password") ?? "").trim();
    const roomCode = String(form.get("roomCode") ?? "").trim().toUpperCase();
    const maxPlayerCount = Number(form.get("maxPlayerCount") ?? 0);

    if (!name || !password || (!isCreate && !roomCode) || (isCreate && !maxPlayerCount)) {
        return;
    }

    const data = isCreate ? await createRoom(name, maxPlayerCount, password) : await joinRoom(roomCode, name, password);

    sessionStorage.setItem("roomId", String(data.roomId));
    sessionStorage.setItem("isLobbyLeader", String(isCreate));
    sessionStorage.setItem("token", data.token);
    sessionStorage.setItem("roomPassword", password);

    navigate(`/room/${data.roomCode}`);
  }

  return (
    <>
      <form onSubmit={(e) => handleRoom(e, false)}>
        <TextInput
          name="name"
          placeholder="Enter your name"
        />
        <TextInput
          name="roomCode"
          placeholder="Enter room code"
        />
        <TextInput
          name="password"
          placeholder="Enter room password"
        />
        <button type="submit">Join Room</button>
      </form>
      <form onSubmit={(e) => handleRoom(e, true)}>
        <TextInput
          name="name"
          placeholder="Enter your name"
        />
        <TextInput
          name="maxPlayerCount"
          placeholder="Enter max player count"
          type="number"
          validate={(value) => Number(value) > 1}
        />
        <TextInput
          name="password"
          placeholder="Choose a room password"
        />
        <button type="submit">Create Room</button>
      </form>
    </>
  )
}

export default Home
