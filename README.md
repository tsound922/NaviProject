# NaviProject

A local LLM-powered personal knowledge base system that helps you review past tasks and accumulate work experience.

## Features

- 📝 **Document Ingestion**: Vectorize and store task notes and work records into the knowledge base
- 🔍 **Semantic Search**: Retrieve relevant historical records based on vector similarity
- 💬 **Conversational Q&A**: Leverage RAG to let the local model answer questions based on your knowledge base
- 🗂️ **Session Management**: Save complete conversation history with multi-session support
- 🔒 **Fully Local**: Models and data run entirely on your machine — no privacy concerns

## Tech Stack

### Backend
- **Framework**: ASP.NET Core Web API (.NET 8)
- **Database**: PostgreSQL + pgvector (vector storage)
- **ORM**: Dapper
- **Local Models**: Ollama
  - Chat model: qwen3:8b
  - Embedding model: bge-m3 (1024 dimensions)

### Frontend
- **Framework**: React + TypeScript (Vite)
- **HTTP Client**: Axios

## Project Structure

```
NaviProject/
├── NaviProject/
│   ├── NaviProject.Api/              # ASP.NET Core Web API
│   │   ├── Controllers/              # ChatController, RagController, ConversationController
│   │   └── Services/                 # OllamaEmbeddingService, OllamaLanguageModelService
│   ├── NaviProject.Core/             # Business logic layer
│   │   ├── Interfaces/               # Interface definitions
│   │   ├── Models/                   # Data models
│   │   └── Services/                 # ChatService, RagService, ConversationService
│   └── NaviProject.Infrastructure/   # Data access layer
│       ├── Repositories/             # ChatRepository, RagRepository
│       └── TypeHandlers/             # Dapper type handlers
└── navi-project-ui/                  # React frontend
    └── src/
        ├── api/                      # API client
        ├── components/               # Sidebar, ChatWindow
        ├── pages/
        └── types/                    # TypeScript type definitions
```

## Database Schema

```sql
-- Knowledge base table
CREATE TABLE simp_rag (
    id SERIAL PRIMARY KEY,
    source TEXT NOT NULL,
    chunk_id TEXT NOT NULL,
    start_index INT NOT NULL,
    end_index INT NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    content TEXT NOT NULL,
    embedding VECTOR(1024)
);

-- Chat session table
CREATE TABLE chat (
    id SERIAL PRIMARY KEY,
    title TEXT,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Chat message table
CREATE TABLE chat_message (
    id SERIAL PRIMARY KEY,
    chat_id INT NOT NULL REFERENCES chat(id) ON DELETE CASCADE,
    role TEXT NOT NULL CHECK (role IN ('user', 'assistant', 'system')),
    content TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);
```

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 22+](https://nodejs.org)
- [Docker](https://www.docker.com) (for running PostgreSQL)
- [Ollama](https://ollama.com)

## Getting Started

### 1. Start PostgreSQL

```bash
docker run -d \
  --name postgres \
  -e POSTGRES_PASSWORD=yourpassword \
  -p 5432:5432 \
  pgvector/pgvector:pg16
```

### 2. Initialize the Database

Connect to PostgreSQL and execute the SQL statements in `schema.sql`.

### 3. Pull Ollama Models

```bash
ollama pull qwen3:8b
ollama pull bge-m3
```

### 4. Configure the Backend

Edit `NaviProject/NaviProject.Api/appsettings.json`:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=your_db;Username=your_user;Password=your_password"
  },
  "Ollama": {
    "BaseUrl": "http://localhost:11434"
  }
}
```

### 5. Start the Backend

```bash
cd NaviProject/NaviProject.Api
dotnet run
```

The backend runs at `http://localhost:5289` by default. Swagger docs available at `http://localhost:5289/swagger`.

### 6. Start the Frontend

```bash
cd navi-project-ui
npm install
npm run dev
```

The frontend runs at `http://localhost:5173` by default.

## API Reference

| Method | Path | Description |
|--------|------|-------------|
| POST | `/api/chat` | Create a new chat session |
| GET | `/api/chat` | Get all chat sessions |
| GET | `/api/chat/{chatId}/messages` | Get messages for a session |
| DELETE | `/api/chat/{chatId}` | Delete a chat session |
| POST | `/api/conversation/{chatId}` | Send a message (RAG + chat) |
| POST | `/api/rag/ingest` | Ingest a document into the knowledge base |
| GET | `/api/rag/search` | Semantic search the knowledge base |
| DELETE | `/api/rag/{source}` | Delete a knowledge base source |

## Roadmap

- [ ] Document ingestion UI
- [ ] Jira / Confluence integration
- [ ] Authentication and multi-user support
- [ ] Codebase ingestion
- [ ] MCP Server integration
- [ ] Server deployment

## License

MIT
