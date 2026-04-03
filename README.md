# NaviProject

A local LLM-powered personal knowledge base system that helps you review past tasks and accumulate work experience.

## Features

- 📝 **Document Ingestion**: Vectorize and store task notes and work records into the knowledge base
- 🔍 **Semantic Search**: Retrieve relevant historical records based on vector similarity
- 💬 **Conversational Q&A**: Leverage RAG to let the local model answer questions based on your knowledge base
- 🗂️ **Session Management**: Save complete conversation history with multi-session support
- 👤 **User Authentication**: JWT-based login with full multi-user data isolation
- 🔒 **Fully Local**: Models and data run entirely on your machine — no privacy concerns

## Tech Stack

### Backend
- **Framework**: ASP.NET Core Web API (.NET 8)
- **Database**: PostgreSQL + pgvector (vector storage)
- **ORM**: Dapper
- **Auth**: JWT (BCrypt password hashing)
- **Local Models**: Ollama
  - Chat model: qwen3:8b
  - Embedding model: nomic-embed-text (768 dimensions)

### Frontend
- **Framework**: React + TypeScript (Vite)
- **HTTP Client**: Axios

## Project Structure

```
NaviProject/
├── NaviProject/
│   ├── NaviProject.Api/              # ASP.NET Core Web API
│   │   ├── Controllers/              # AuthController, ChatController, RagController, ConversationController
│   │   ├── Extensions/               # ClaimsPrincipalExtensions
│   │   └── Services/                 # OllamaEmbeddingService, OllamaLanguageModelService, AuthService
│   ├── NaviProject.Core/             # Business logic layer
│   │   ├── Interfaces/               # IAuthService, IChatRepository, IChatMessageRepository, IRagRepository, IUserRepository, IEmbeddingService, ILanguageModelService
│   │   ├── Models/                   # AppUser, Chat, ChatMessage, RagChunk
│   │   └── Services/                 # ChatService, RagService, ConversationService, UserService
│   └── NaviProject.Infrastructure/   # Data access layer
│       ├── Repositories/             # UserRepository, ChatRepository, ChatMessageRepository, RagRepository
│       └── TypeHandlers/             # VectorTypeHandler
└── navi-project-ui/                  # React frontend
    └── src/
        ├── api/                      # API client
        ├── components/               # Sidebar, ChatWindow, IngestPanel
        ├── pages/                    # LoginPage
        └── types/                    # TypeScript type definitions
```

## Database Schema

```sql
-- User table
CREATE TABLE app_user (
    id SERIAL PRIMARY KEY,
    username TEXT NOT NULL UNIQUE,
    email TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- Knowledge base table
CREATE TABLE simp_rag (
    id SERIAL PRIMARY KEY,
    user_id INT REFERENCES app_user(id) ON DELETE CASCADE,
    source TEXT NOT NULL,
    chunk_id TEXT NOT NULL,
    start_index INT NOT NULL,
    end_index INT NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW(),
    content TEXT NOT NULL,
    embedding VECTOR(768)
);

-- Chat session table
CREATE TABLE chat (
    id SERIAL PRIMARY KEY,
    user_id INT REFERENCES app_user(id) ON DELETE CASCADE,
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

Connect to PostgreSQL and execute the schema SQL statements.

### 3. Pull Ollama Models

```bash
ollama pull qwen3:8b
ollama pull nomic-embed-text
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
  },
  "Jwt": {
    "Secret": "your-super-secret-key-at-least-32-characters-long"
  }
}
```

### 5. Start the Backend

```bash
cd NaviProject/NaviProject.Api
dotnet run
```

The backend runs at `http://localhost:5289` by default. Swagger docs: `http://localhost:5289/swagger`

### 6. Start the Frontend

```bash
cd navi-project-ui
npm install
npm run dev
```

The frontend runs at `http://localhost:5173` by default.

## API Reference

| Method | Path | Auth | Description |
|--------|------|------|-------------|
| POST | `/api/auth/register` | ❌ | Register a new user |
| POST | `/api/auth/login` | ❌ | User login |
| POST | `/api/chat` | ✅ | Create a new chat session |
| GET | `/api/chat` | ✅ | Get all chat sessions |
| GET | `/api/chat/{chatId}/messages` | ✅ | Get messages for a session |
| DELETE | `/api/chat/{chatId}` | ✅ | Delete a chat session |
| POST | `/api/conversation/{chatId}` | ✅ | Send a message (RAG + chat) |
| POST | `/api/rag/ingest` | ✅ | Ingest a document into the knowledge base |
| GET | `/api/rag/search` | ✅ | Semantic search the knowledge base |
| DELETE | `/api/rag/{source}` | ✅ | Delete a knowledge base source |

## Roadmap

- [x] Core architecture setup
- [x] RAG document ingestion and semantic search
- [x] RAG + conversational Q&A
- [x] Document ingestion UI
- [x] User authentication (JWT)
- [x] Multi-user data isolation
- [ ] Jira integration
- [ ] Confluence integration
- [ ] Azure DevOps integration
- [ ] Azure AD login
- [ ] Server deployment
- [ ] MCP Server integration

## License

MIT
