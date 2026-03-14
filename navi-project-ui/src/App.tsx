import { useState } from 'react';
import Sidebar from './component/Sidebar';
import ChatWindow from './component/ChatWindow';
import IngestPanel from './component/IngestPanel';
import { chatApi } from './api/client';

type View = 'chat' | 'ingest';

export default function App() {
  const [selectedChatId, setSelectedChatId] = useState<number | null>(null);
  const [view, setView] = useState<View>('chat');

  const handleNewChat = async () => {
    const res = await chatApi.createChat();
    setSelectedChatId(res.data.chatId);
    setView('chat');
  };

  return (
    <div style={{ display: 'flex', height: '100vh', overflow: 'hidden' }}>
      <Sidebar
        selectedChatId={selectedChatId}
        onSelectChat={(id) => { setSelectedChatId(id); setView('chat'); }}
        onNewChat={handleNewChat}
        onIngest={() => setView('ingest')}
      />
      <div style={{ flex: 1, display: 'flex', flexDirection: 'column' }}>
        {view === 'chat' ? (
          <ChatWindow chatId={selectedChatId} />
        ) : (
          <IngestPanel />
        )}
      </div>
    </div>
  );
}