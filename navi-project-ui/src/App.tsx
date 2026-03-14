import { useState } from 'react';
import Sidebar from './component/Sidebar';
import ChatWindow from './component/ChatWindow';
import IngestPanel from './component/IngestPanel';
import LoginPage from './pages/LoginPage';
import { chatApi } from './api/client';

type View = 'chat' | 'ingest';

export default function App() {
  const [isLoggedIn, setIsLoggedIn] = useState<boolean>(!!localStorage.getItem('token'));
  const [selectedChatId, setSelectedChatId] = useState<number | null>(null);
  const [view, setView] = useState<View>('chat');

  const handleLoginSuccess = () => {
    setIsLoggedIn(true);
  };

  const handleLogout = () => {
    localStorage.removeItem('token');
    setIsLoggedIn(false);
    setSelectedChatId(null);
  };

  const handleNewChat = async () => {
    const res = await chatApi.createChat();
    setSelectedChatId(res.data.chatId);
    setView('chat');
  };

  if (!isLoggedIn) {
    return <LoginPage onLoginSuccess={handleLoginSuccess} />;
  }

  return (
    <div style={{ display: 'flex', height: '100vh', overflow: 'hidden' }}>
      <Sidebar
        selectedChatId={selectedChatId}
        onSelectChat={(id) => { setSelectedChatId(id); setView('chat'); }}
        onNewChat={handleNewChat}
        onIngest={() => setView('ingest')}
        onLogout={handleLogout}
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