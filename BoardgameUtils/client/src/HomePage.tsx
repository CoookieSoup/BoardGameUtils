import { useNavigate } from 'react-router-dom'
import './App.css'
import AsyncForm from './components/AsyncForm'
import TextInput from './components/inputs'
import { createRoom, joinRoom } from './requests/roomsApi'

function Home() {
  const navigate = useNavigate()

  async function handleRoom(formElement: HTMLFormElement, isCreate: boolean) {
    const form = new FormData(formElement);
    const name = String(form.get("name") ?? "").trim();
    const password = String(form.get("password") ?? "").trim();
    const roomCode = String(form.get("roomCode") ?? "").trim().toUpperCase();
    const maxPlayerCount = Number(form.get("maxPlayerCount") ?? 0);

    if (!name || !password || (!isCreate && !roomCode) || (isCreate && !maxPlayerCount)) {
        return;
    }

    const data = isCreate ? await createRoom(name, maxPlayerCount, password) : await joinRoom(roomCode, name, password);

    sessionStorage.setItem("roomId", String(data.roomId));
    sessionStorage.setItem("playerId", String(data.playerId));
    sessionStorage.setItem("isLobbyLeader", String(isCreate));
    sessionStorage.setItem("token", data.token);
    sessionStorage.setItem("roomPassword", password);

    navigate(`/room/${data.roomCode}`);
  }

  return (
    <div className="page">
      <h1 className="title">Board Game Utils</h1>
      <div className="home-forms">
      <AsyncForm className="panel" submitLabel="Join Room" onSubmit={(form) => handleRoom(form, false)}>
        <h2>Join a Room</h2>
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
      </AsyncForm>
      <AsyncForm className="panel" submitLabel="Create Room" onSubmit={(form) => handleRoom(form, true)}>
        <h2>Create a Room</h2>
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
      </AsyncForm>
      </div>
    </div>
  )
}

export default Home
