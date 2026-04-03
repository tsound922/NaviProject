# NaviProject

A local LLM-powered enterprise knowledge base system that unifies information from Jira, Confluence, and Azure DevOps, helping teams retrieve historical tasks and accumulate work experience.

## Features

- 📝 **Document Ingestion**: Vectorize and store task notes and work records into the knowledge base
- 🔍 **Hybrid Search**: Combines semantic search and full-text search, with exact Jira ticket key lookup
- 💬 **Conversational Q&A**: Leverage RAG to answer questions based on knowledge base content, with relevant links included in responses
- 🗂️ **Session Management**: Save complete conversation history with multi-session support
- 👤 **User Authentication**: JWT-based login with full multi-user data isolation
- 🔒 **Public/Private Knowledge Base**: Personal private knowledge base alongside shared team knowledge base
- 🎫 **Jira Integration**: Auto-sync Jira tickets including comments, status, assignee, and attachment info
- 🌐 **Multilingual Support**: Automatically responds in the user's language (English/Chinese)
- 🔒 **Fully Local**: Models and data run entirely on your machine — no privacy concerns

## Tech Stack

### Backend
- **Framework**: ASP.NET Core Web API (.NET 8)
- **Database**: PostgreSQL + pgvector (vector storage) + full-text search
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
├── NaviBackend/
│   ├── NaviProject.Api/              # ASP.NET Core Web API
│   │   ├── Controllers/              # AuthController, ChatController, RagController, ConversationController, JiraController
│   │   ├── Extensions/               # ClaimsPrincipalExtensions
│   │   └── Services/                 # OllamaEmbeddingService, OllamaLanguageModelService, AuthService, JiraService
│   ├── NaviProject.Core/             # Business logic layer
│   │   ├── Interfaces/               # IAuthService, IChatRepository, IChatMessageRepository, IRagRepository, IUserRepository, IEmbeddingService, ILanguageModelService, IJiraService
│   │   ├── Models/                   # AppUser, Chat, ChatMessage, RagChunk, JiraTicket
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
    embedding VECTOR(768),
    is_public BOOLEAN DEFAULT FALSE,
    metadata JSONB,
    content_tsv tsvector GENERATED ALWAYS AS (to_tsvector('english', content)) STORED
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

In Visual Studio, right-click `NaviProject.Api` → **Manage User Secrets** and fill in:

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=your_db;Username=your_user;Password=your_password"
  },
  "Jwt": {
    "Secret": "your-super-secret-key-at-least-32-characters-long"
  },
  "Jira": {
    "BaseUrl": "https://yourcompany.atlassian.net",
    "Email": "your-email@company.com",
    "ApiToken": "your-api-token"
  }
}
```

### 5. Start the Backend

```bash
cd NaviBackend/NaviProject.Api
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
| POST | `/api/rag/ingest` | ✅ | Ingest document into private knowledge base |
| POST | `/api/rag/ingest/public` | ✅ | Ingest document into public knowledge base |
| GET | `/api/rag/search` | ✅ | Hybrid search the knowledge base |
| DELETE | `/api/rag/{source}` | ✅ | Delete a knowledge base source |
| GET | `/api/jira/tickets` | ✅ | Fetch Jira tickets |
| POST | `/api/jira/sync` | ✅ | Sync Jira tickets to knowledge base |

## Roadmap

- [x] Core architecture setup
- [x] RAG document ingestion and semantic search
- [x] RAG + conversational Q&A
- [x] Document ingestion UI
- [x] User authentication (JWT)
- [x] Multi-user data isolation
- [x] Public/private knowledge base
- [x] Jira integration (auto-sync, metadata, ticket URL)
- [x] Hybrid search (semantic + full-text)
- [x] Multilingual support
- [ ] Confluence integration
- [ ] Azure DevOps integration (Git commits, PR history)
- [ ] Conversation-triggered knowledge ingestion
- [ ] Multi-agent architecture (Semantic Kernel)
- [ ] Azure AD login
- [ ] UI upgrade (Shadcn/ui)
- [ ] Server deployment
- [ ] MCP Server integration

## License

MIT
