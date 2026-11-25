import { useEffect, useState } from 'react';
import type { ChatMessageResponse, PlayerInfoResponse } from '../requests/roomsApi';
import { loadMessages, sendMessage } from '../requests/roomsApi';
import { connection } from "../signalr";
import AsyncForm from './AsyncForm';
import TextInput from './inputs';

type ChatProps = {
  code: string;
  playerId: number;
  players: PlayerInfoResponse[];
};

function Chat({ code, playerId, players }: ChatProps) {
  const [messages, setMessages] = useState<ChatMessageResponse[]>([]);
  const [recipientId, setRecipientId] = useState(0);

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

  const handleSend = async (form: HTMLFormElement) => {
    const text = String(new FormData(form).get("text") ?? "");

    await sendMessage(code, text, recipientId || null);
    form.reset();
  };

  const myNickname = players.find(player => player.id === playerId)?.nickname;
  const tabs = [
    { id: 0, nickname: "Everyone" },
    ...players.filter(player => player.id !== playerId),
  ];

  const openTab = tabs.find(tab => tab.id === recipientId) ?? tabs[0];

  const visibleMessages = messages.filter(message =>
    openTab.id === 0
      ? message.recipientNickname === null
      : (message.senderNickname === openTab.nickname && message.recipientNickname === myNickname)
        || (message.senderNickname === myNickname && message.recipientNickname === openTab.nickname)
  );

  return (
    <div className="panel chat">
      <h2>Table Talk</h2>
      <div className="chat-tabs">
        {tabs.map(tab => (
          <button
            key={tab.id}
            className={tab === openTab ? "chat-tab active" : "chat-tab"}
            onClick={() => setRecipientId(tab.id)}
          >
            {tab.nickname}
          </button>
        ))}
      </div>
      <div className="chat-messages">
        {visibleMessages.map(message => (
          <div key={message.id} className={message.senderNickname === myNickname ? "message mine" : "message"}>
            <div className="message-meta">
              <b>{message.senderNickname}</b>
              {" · "}
              {new Date(message.sentAt).toTimeString().slice(0, 5)}
            </div>
            <div>{message.text}</div>
          </div>
        ))}
      </div>
      <AsyncForm className="chat-form" submitLabel="Send" onSubmit={handleSend}>
        <TextInput
          name="text"
          placeholder={`Message ${openTab.nickname}`}
        />
      </AsyncForm>
    </div>
  );
}

export default Chat;
