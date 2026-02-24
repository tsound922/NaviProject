import { useState } from 'react';
import Sidebar from './component/Sidebar';
import ChatWindow from './component/ChatWindow';
import { chatApi } from './api/client';

export default function App() {
  const [selectedChatId, setSelectedChatId] = useState<number | null>(null);

  const handleNewChat = async () => {
    const res = await chatApi.createChat();
    setSelectedChatId(res.data.chatId);
  };

  return (
    <div style={{
      display: 'flex',
      height: '100vh',
      overflow: 'hidden',
    }}>
      <Sidebar
        selectedChatId={selectedChatId}
        onSelectChat={setSelectedChatId}
        onNewChat={handleNewChat}
      />
      <ChatWindow chatId={selectedChatId} />
    </div>
  );
}