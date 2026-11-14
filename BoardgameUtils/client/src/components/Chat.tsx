import { useEffect, useState } from 'react';
import type { ChatMessageResponse, PlayerInfoResponse } from '../requests/roomsApi';
import { loadMessages, sendMessage } from '../requests/roomsApi';
import { connection } from "../signalr";
import TextInput from './inputs';

type ChatProps = {
  code: string;
  playerId: number;
  players: PlayerInfoResponse[];
};

function Chat({ code, playerId, players }: ChatProps) {
  const [messages, setMessages] = useState<ChatMessageResponse[]>([]);
  const [recipientId, setRecipientId] = useState("");

  useEffect(() => {
    const refresh = async () => {
      const data = await loadMessages(code);
      setMessages(data);
    };

    connection.on("ChatChanged", refresh);
    refresh();

    return () => {
      connection.off("ChatChanged", refresh);
    };
  }, [code]);

  const handleSend = async (e: React.SubmitEvent<HTMLFormElement>) => {
    e.preventDefault();

    const form = e.currentTarget;
    const text = String(new FormData(form).get("text") ?? "");

    await sendMessage(code, text, recipientId ? Number(recipientId) : null);
    form.reset();
  };

  return (
    <div>
      {messages.map(message => (
        <div key={message.id}>
          {message.senderNickname}
          {message.recipientNickname && ` to ${message.recipientNickname} (private)`}: {message.text}
        </div>
      ))}
      <form onSubmit={handleSend}>
        <select value={recipientId} onChange={(e) => setRecipientId(e.target.value)}>
          <option value="">Everyone</option>
          {players.filter(player => player.id !== playerId).map(player => (
            <option key={player.id} value={player.id}>{player.nickname}</option>
          ))}
        </select>
        <TextInput
          name="text"
          placeholder="Write a message"
        />
        <button type="submit">Send</button>
      </form>
    </div>
  );
}

export default Chat;
