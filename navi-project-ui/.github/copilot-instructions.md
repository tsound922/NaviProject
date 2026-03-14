# Copilot Instructions for `navi-project-ui`

## 📌 Project Overview
This is a **React + TypeScript + Vite** frontend that connects to a local backend at `http://localhost:5289/api`. It is a small chat/QA UI with 3 main areas:

- `src/App.tsx` - root layout (sidebar + chat window)
- `src/component/Sidebar.tsx` - lists chats, creates/deletes chats via `chatApi`
- `src/component/ChatWindow.tsx` - shows messages, sends user input via `conversationApi`

There is also an `IngestPanel` component (`src/component/IngestPanel.tsx`) that isn't currently wired into the UI.

## 🧭 Architecture & Data Flow (Key Files)

- **API layer:** `src/api/client.ts`
  - exports `chatApi`, `conversationApi`, `ragApi`.
  - Uses Axios and targets `http://localhost:5289/api`.
  - Endpoints expected by the backend:
    - `POST /chat` → creates a chat
    - `GET /chat` → list chats
    - `GET /chat/:id/messages` → chat messages
    - `DELETE /chat/:id` → delete chat
    - `POST /conversation/:id` → send message
    - `POST /rag/ingest`, `GET /rag/search` → ingestion/search

- **Types:** `src/types/index.ts` contains shared shape definitions (`Chat`, `ChatMessage`, `RagChunk`).

- **UI Layer:** uses inline styles (no CSS frameworks). Look for style objects in the component files.

## 🧩 Patterns & Conventions

- **Functional components + hooks** (no class components).
- **Inline styling** (no CSS modules or styled components).
- **Optimistic UI patterns:** `ChatWindow` appends a user message immediately before awaiting backend response.
- **Cancel-safe async effects:** use `cancelled` flag in `useEffect` before setting state.

## 🚀 Common Dev Commands

From the repo root:

- `npm install` — install dependencies
- `npm run dev` — run Vite dev server (hot reload)
- `npm run build` — build production bundle (`tsc -b && vite build`)
- `npm run preview` — preview production build
- `npm run lint` — run ESLint

> ⚠️ The UI expects a backend running at `http://localhost:5289/api`. If it’s not running, API calls will fail.

## 🔧 Adding New Features / Components

- Add new pages/components under `src/component/`.
- If routing is needed, add `react-router-dom` usage (it’s already a dependency, though not currently used).
- For new API endpoints, update `src/api/client.ts` and add types in `src/types/index.ts`.

## 🪧 Notes for Copilot / AI Agents

- Keep modifications scoped: the UI is small; changes should be minimal and aligned with existing patterns (hooks + inline styles).
- If you change the API surface in `src/api/client.ts`, ensure it matches the backend contract (`/api/chat`, `/api/conversation`, `/api/rag`).
- Avoid adding heavy dependencies unless the repo explicitly wants them; this project is intentionally minimal.

---

If any part of the architecture is unclear (e.g., why `IngestPanel` is not mounted), ask for clarification — the UI is a small demo scaffold rather than a full product.
