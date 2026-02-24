export interface Chat {
  id: number;
  title: string | null;
  createdAt: string;
  updatedAt: string;
}

export interface ChatMessage {
  id: number;
  chatId: number;
  role: "user" | "assistant" | "system";
  content: string;
  createdAt: string;
}

export interface RagChunk {
  id: number;
  source: string;
  chunkId: string;
  startIndex: number;
  endIndex: number;
  content: string;
  createdAt: string;
  updatedAt: string;
}
