import axios from "axios";
import type { Chat, ChatMessage, RagChunk } from "../types/index";

const client = axios.create({
  baseURL: "http://localhost:5289/api",
  headers: {
    "Content-Type": "application/json",
  },
});

//JWT Token
client.interceptors.request.use((config) => {
  const token = localStorage.getItem("token");
  if (token) {
    config.headers.Authorization = `Bearer ${token}`;
  }
  return config;
});

export const chatApi = {
  createChat: (title?: string) =>
    client.post<{ chatId: number }>("/chat", JSON.stringify(title ?? null)),
  getAllChats: () => client.get<Chat[]>("/chat"),
  getMessages: (chatId: number) =>
    client.get<ChatMessage[]>(`/chat/${chatId}/messages`),
  deleteChat: (chatId: number) => client.delete(`/chat/${chatId}`),
};

export const conversationApi = {
  sendMessage: (chatId: number, message: string) =>
    client.post<{ response: string }>(`/conversation/${chatId}`, { message }),
};

export const ragApi = {
  ingest: (source: string, text: string) =>
    client.post("/rag/ingest", { source, text }),
  search: (query: string, topK: number = 5) =>
    client.get<RagChunk[]>("/rag/search", { params: { query, topK } }),
};

export const authApi = {
  register: (username: string, email: string, password: string) =>
    client.post<{ message: string; token: string }>("/auth/register", {
      username,
      email,
      password,
    }),
  login: (username: string, password: string) =>
    client.post<{ message: string; token: string }>("/auth/login", {
      username,
      password,
    }),
};
