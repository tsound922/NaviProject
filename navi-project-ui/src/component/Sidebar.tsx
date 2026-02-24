import { useEffect, useState } from 'react';
import type { Chat } from '../types/index';
import { chatApi } from '../api/client';

interface SidebarProps {
  selectedChatId: number | null;
  onSelectChat: (chatId: number) => void;
  onNewChat: () => void;
}

export default function Sidebar({ selectedChatId, onSelectChat, onNewChat }: SidebarProps) {
  const [chats, setChats] = useState<Chat[]>([]);

  const loadChats = async () => {
    const res = await chatApi.getAllChats();
    setChats(res.data);
  };

  useEffect(() => {
  let cancelled = false;

  const fetchChats = async () => {
    const res = await chatApi.getAllChats();
    if (!cancelled) {
      setChats(res.data);
    }
  };

  fetchChats();

  return () => {
    cancelled = true;
  };
}, [selectedChatId]);

  const handleDelete = async (e: React.MouseEvent, chatId: number) => {
    e.stopPropagation();
    await chatApi.deleteChat(chatId);
    await loadChats();
  };

  return (
    <div style={{
      width: '260px',
      height: '100vh',
      backgroundColor: '#1e1e1e',
      color: '#fff',
      display: 'flex',
      flexDirection: 'column',
      padding: '16px',
      gap: '8px',
    }}>
      <button
        onClick={onNewChat}
        style={{
          backgroundColor: '#4f46e5',
          color: '#fff',
          border: 'none',
          borderRadius: '8px',
          padding: '10px',
          cursor: 'pointer',
          fontWeight: 'bold',
          marginBottom: '8px',
        }}
      >
        + 新建会话
      </button>

      <div style={{ overflowY: 'auto', flex: 1 }}>
        {chats.map(chat => (
          <div
            key={chat.id}
            onClick={() => onSelectChat(chat.id)}
            style={{
              padding: '10px',
              borderRadius: '8px',
              cursor: 'pointer',
              backgroundColor: selectedChatId === chat.id ? '#3f3f3f' : 'transparent',
              display: 'flex',
              justifyContent: 'space-between',
              alignItems: 'center',
              marginBottom: '4px',
            }}
          >
            <span style={{ fontSize: '14px', overflow: 'hidden', textOverflow: 'ellipsis', whiteSpace: 'nowrap' }}>
              {chat.title ?? '新会话'}
            </span>
            <button
              onClick={(e) => handleDelete(e, chat.id)}
              style={{
                background: 'none',
                border: 'none',
                color: '#aaa',
                cursor: 'pointer',
                fontSize: '16px',
              }}
            >
              ×
            </button>
          </div>
        ))}
      </div>
    </div>
  );
}