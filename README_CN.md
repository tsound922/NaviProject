# NaviProject

一个基于本地 LLM 的个人知识库系统，帮助你回顾历史任务、沉淀工作经验。

## 功能特性

- 📝 **文档录入**：将任务笔记、工作记录向量化存储到知识库
- 🔍 **语义搜索**：基于向量相似度检索相关历史记录
- 💬 **对话问答**：结合 RAG 技术，让本地模型根据知识库内容回答问题
- 🗂️ **会话管理**：保存完整对话历史，支持多会话切换
- 🔒 **完全本地**：模型和数据均在本地运行，无需担心数据隐私

## 技术栈

### 后端
- **框架**：ASP.NET Core Web API (.NET 8)
- **数据库**：PostgreSQL + pgvector（向量存储）
- **ORM**：Dapper
- **本地模型**：Ollama
  - 对话模型：qwen3:8b
  - Embedding 模型：bge-m3（1024 维）

### 前端
- **框架**：React + TypeScript（Vite）
- **HTTP 客户端**：Axios

## 项目结构

```
NaviProject/
├── NaviProject/
│   ├── NaviProject.Api/          # ASP.NET Core Web API
│   │   ├── Controllers/          # ChatController, RagController, ConversationController
│   │   └── Services/             # OllamaEmbeddingService, OllamaLanguageModelService
│   ├── NaviProject.Core/         # 业务逻辑层
│   │   ├── Interfaces/           # 接口定义
│   │   ├── Models/               # 数据模型
│   │   └── Services/             # ChatService, RagService, ConversationService
│   └── NaviProject.Infrastructure/  # 数据访问层
│       ├── Repositories/         # ChatRepository, RagRepository
│       └── TypeHandlers/         # Dapper 类型处理
└── navi-project-ui/              # React 前端
    └── src/
        ├── api/                  # API 客户端
        ├── components/           # Sidebar, ChatWindow
        ├── pages/
        └── types/                # TypeScript 类型定义
```

## 数据库结构

```sql
-- 知识库表
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

-- 会话表
CREATE TABLE chat (
    id SERIAL PRIMARY KEY,
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

连接到 PostgreSQL 并执行 `schema.sql` 中的建表语句。

### 3. 拉取 Ollama 模型

```bash
ollama pull qwen3:8b
ollama pull bge-m3
```

### 4. 配置后端

编辑 `NaviProject/NaviProject.Api/appsettings.json`：

```json
{
  "ConnectionStrings": {
    "Postgres": "Host=localhost;Port=5432;Database=你的数据库名;Username=你的用户名;Password=你的密码"
  },
  "Ollama": {
    "BaseUrl": "http://localhost:11434"
  }
}
```

### 5. 启动后端

```bash
cd NaviProject/NaviProject.Api
dotnet run
```

后端默认运行在 `http://localhost:5289`，Swagger 文档地址：`http://localhost:5289/swagger`

### 6. 启动前端

```bash
cd navi-project-ui
npm install
npm run dev
```

前端默认运行在 `http://localhost:5173`

## API 接口

| 方法 | 路径 | 说明 |
|------|------|------|
| POST | `/api/chat` | 创建新会话 |
| GET | `/api/chat` | 获取所有会话 |
| GET | `/api/chat/{chatId}/messages` | 获取会话消息 |
| DELETE | `/api/chat/{chatId}` | 删除会话 |
| POST | `/api/conversation/{chatId}` | 发送消息（RAG + 对话） |
| POST | `/api/rag/ingest` | 录入文档到知识库 |
| GET | `/api/rag/search` | 语义搜索知识库 |
| DELETE | `/api/rag/{source}` | 删除知识库来源 |

## 路线图

- [ ] 文档录入 UI
- [ ] Jira / Confluence 接入
- [ ] 登录和多用户支持
- [ ] 代码库导入
- [ ] MCP Server 改造
- [ ] 部署到服务器

## License

MIT
