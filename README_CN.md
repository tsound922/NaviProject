# NaviProject

一个基于本地 LLM 的企业知识库系统，帮助团队统一检索 Jira、Confluence、Azure DevOps 等平台的信息，回顾历史任务、沉淀工作经验。

## 功能特性

- 📝 **文档录入**：将任务笔记、工作记录向量化存储到知识库
- 🔍 **混合搜索**：结合语义搜索和全文搜索，支持 Jira ticket 编号精确查询
- 💬 **对话问答**：结合 RAG 技术，让本地模型根据知识库内容回答问题，并附上相关链接
- 🗂️ **会话管理**：保存完整对话历史，支持多会话切换
- 👤 **用户认证**：JWT 登录认证，多用户数据完全隔离
- 🔒 **公私知识库**：支持个人私有知识库和团队公共知识库
- 🎫 **Jira 接入**：自动同步 Jira tickets，包含评论、状态、assignee、附件信息
- 🌐 **多语言支持**：自动跟随用户语言（中文/英文）回答
- 🔒 **完全本地**：模型和数据均在本地运行，无需担心数据隐私

## 技术栈

### 后端
- **框架**：ASP.NET Core Web API (.NET 8)
- **数据库**：PostgreSQL + pgvector（向量存储）+ 全文搜索
- **ORM**：Dapper
- **认证**：JWT（BCrypt 密码加密）
- **本地模型**：Ollama
  - 对话模型：qwen3:8b
  - Embedding 模型：nomic-embed-text（768 维）

### 前端
- **框架**：React + TypeScript（Vite）
- **HTTP 客户端**：Axios

## 项目结构

```
NaviProject/
├── Solution1/
│   ├── NaviProject.Api/              # ASP.NET Core Web API
│   │   ├── Controllers/              # AuthController, ChatController, RagController, ConversationController, JiraController
│   │   ├── Extensions/               # ClaimsPrincipalExtensions
│   │   └── Services/                 # OllamaEmbeddingService, OllamaLanguageModelService, AuthService, JiraService
│   ├── NaviProject.Core/             # 业务逻辑层
│   │   ├── Interfaces/               # IAuthService, IChatRepository, IChatMessageRepository, IRagRepository, IUserRepository, IEmbeddingService, ILanguageModelService, IJiraService
│   │   ├── Models/                   # AppUser, Chat, ChatMessage, RagChunk, JiraTicket
│   │   └── Services/                 # ChatService, RagService, ConversationService, UserService
│   └── NaviProject.Infrastructure/   # 数据访问层
│       ├── Repositories/             # UserRepository, ChatRepository, ChatMessageRepository, RagRepository
│       └── TypeHandlers/             # VectorTypeHandler
└── navi-project-ui/                  # React 前端
    └── src/
        ├── api/                      # API 客户端
        ├── components/               # Sidebar, ChatWindow, IngestPanel
        ├── pages/                    # LoginPage
        └── types/                    # TypeScript 类型定义
```

## 数据库结构

```sql
-- 用户表
CREATE TABLE app_user (
    id SERIAL PRIMARY KEY,
    username TEXT NOT NULL UNIQUE,
    email TEXT NOT NULL UNIQUE,
    password_hash TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- 知识库表
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

-- 会话表
CREATE TABLE chat (
    id SERIAL PRIMARY KEY,
    user_id INT REFERENCES app_user(id) ON DELETE CASCADE,
    title TEXT,
    created_at TIMESTAMP DEFAULT NOW(),
    updated_at TIMESTAMP DEFAULT NOW()
);

-- 消息表
CREATE TABLE chat_message (
    id SERIAL PRIMARY KEY,
    chat_id INT NOT NULL REFERENCES chat(id) ON DELETE CASCADE,
    role TEXT NOT NULL CHECK (role IN ('user', 'assistant', 'system')),
    content TEXT NOT NULL,
    created_at TIMESTAMP DEFAULT NOW()
);
```

## 环境要求

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 22+](https://nodejs.org)
- [Docker](https://www.docker.com)（运行 PostgreSQL）
- [Ollama](https://ollama.com)

## 快速开始

### 1. 启动 PostgreSQL

```bash
docker run -d \
  --name postgres \
  -e POSTGRES_PASSWORD=yourpassword \
  -p 5432:5432 \
  pgvector/pgvector:pg16
```

### 2. 初始化数据库

连接到 PostgreSQL 并执行建表语句。

### 3. 拉取 Ollama 模型

```bash
ollama pull qwen3:8b
ollama pull nomic-embed-text
```

### 4. 配置后端

在 Visual Studio 中右键 `NaviProject.Api` → **Manage User Secrets**，填入：

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=你的数据库名;Username=你的用户名;Password=你的密码"
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

### 5. 启动后端

```bash
cd Solution1/NaviProject.Api
dotnet run
```

后端默认运行在 `http://localhost:5289`，Swagger 文档：`http://localhost:5289/swagger`

### 6. 启动前端

```bash
cd navi-project-ui
npm install
npm run dev
```

前端默认运行在 `http://localhost:5173`

## API 接口

| 方法 | 路径 | 认证 | 说明 |
|------|------|------|------|
| POST | `/api/auth/register` | ❌ | 注册新用户 |
| POST | `/api/auth/login` | ❌ | 用户登录 |
| POST | `/api/chat` | ✅ | 创建新会话 |
| GET | `/api/chat` | ✅ | 获取所有会话 |
| GET | `/api/chat/{chatId}/messages` | ✅ | 获取会话消息 |
| DELETE | `/api/chat/{chatId}` | ✅ | 删除会话 |
| POST | `/api/conversation/{chatId}` | ✅ | 发送消息（RAG + 对话） |
| POST | `/api/rag/ingest` | ✅ | 录入文档到私有知识库 |
| POST | `/api/rag/ingest/public` | ✅ | 录入文档到公共知识库 |
| GET | `/api/rag/search` | ✅ | 混合搜索知识库 |
| DELETE | `/api/rag/{source}` | ✅ | 删除知识库来源 |
| GET | `/api/jira/tickets` | ✅ | 获取 Jira tickets |
| POST | `/api/jira/sync` | ✅ | 同步 Jira tickets 到知识库 |

## 路线图

- [x] 基础架构搭建
- [x] RAG 文档录入和语义搜索
- [x] 对话 + RAG 结合
- [x] 文档录入 UI
- [x] 用户登录认证（JWT）
- [x] 多用户数据隔离
- [x] 公共/私有知识库
- [x] Jira 接入（自动同步、metadata、ticket URL）
- [x] 混合搜索（语义 + 全文）
- [x] 多语言支持
- [ ] Confluence 接入
- [ ] Azure DevOps 接入（Git commit、PR 记录）
- [ ] 对话触发知识库录入
- [ ] 多 Agent 架构（Semantic Kernel）
- [ ] Azure AD 登录
- [ ] UI 升级（Shadcn/ui）
- [ ] 部署到服务器
- [ ] MCP Server 改造

## License

MIT
