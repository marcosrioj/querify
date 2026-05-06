# Querify MCP Server

## What this is

Model Context Protocol (MCP) is an open standard that lets AI assistants (Claude, Cursor, VS Code
Copilot, and any MCP-compatible client) call tools, read resources, and use system prompts
provided by an external server. For Querify, this means any AI agent can query spaces and
questions, create drafts, and run the Source → Q&A generation pipeline — all from a conversation.

**Full design:** [`../future/integrations/mcp.md`](../future/integrations/mcp.md)

---

## What works today (TypeScript proxy)

Before `Querify.MCP.Server` is built, a lightweight TypeScript proxy bridges MCP calls to the
existing Querify REST APIs. No changes to the .NET solution required.

### Tools available now

| Tool | Querify endpoint | Auth |
|---|---|---|
| `qna_list_spaces` | `GET /api/qna/space` | `X-Client-Key` |
| `qna_get_space` | `GET /api/qna/space/by-slug/{slug}` | `X-Client-Key` |
| `qna_list_questions` | `GET /api/qna/question` | `X-Client-Key` |
| `qna_get_question` | `GET /api/qna/question/{id}` | `X-Client-Key` |
| `qna_create_question` | `POST /api/qna/question` | `Authorization` + `X-Tenant-Id` |
| `qna_create_answer` | `POST /api/qna/answer` | `Authorization` + `X-Tenant-Id` |
| `qna_create_source` | `POST /api/qna/source` | `Authorization` + `X-Tenant-Id` |
| `qna_link_source` | `POST /api/qna/question/{id}/source` | `Authorization` + `X-Tenant-Id` |
| `tenant_get_workspace` | `GET /api/tenant/tenants/get-all` | `Authorization` + `X-Tenant-Id` |
| `tenant_list_members` | `GET /api/tenant/tenant-users/get-all` | `Authorization` + `X-Tenant-Id` |
| `tenant_get_billing` | `GET /api/tenant/billing/summary` | `Authorization` + `X-Tenant-Id` |

### Quick start

```bash
mkdir querify-mcp && cd querify-mcp
npm init -y
npm install @modelcontextprotocol/sdk zod
npm install -D typescript @types/node tsx
```

Environment variables:

```bash
QUERIFY_PORTAL_API_URL=http://localhost:5010
QUERIFY_PUBLIC_API_URL=http://localhost:5020
QUERIFY_CLIENT_KEY=your-client-key
QUERIFY_TENANT_ID=your-tenant-uuid
QUERIFY_AUTH_TOKEN=your-auth0-token   # only needed for write tools
```

### Connect to Claude Code

`.claude/settings.json` at the project root:

```json
{
  "mcpServers": {
    "querify": {
      "command": "tsx",
      "args": ["./querify-mcp/src/server.ts"],
      "env": {
        "QUERIFY_PORTAL_API_URL": "http://localhost:5010",
        "QUERIFY_PUBLIC_API_URL": "http://localhost:5020",
        "QUERIFY_CLIENT_KEY": "your-client-key"
      }
    }
  }
}
```

### Connect to Claude Desktop

`~/Library/Application Support/Claude/claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "querify": {
      "command": "tsx",
      "args": ["/absolute/path/to/querify-mcp/src/server.ts"],
      "env": {
        "QUERIFY_PORTAL_API_URL": "http://localhost:5010",
        "QUERIFY_PUBLIC_API_URL": "http://localhost:5020",
        "QUERIFY_CLIENT_KEY": "your-client-key"
      }
    }
  }
}
```

### Test with MCP Inspector

```bash
npx @modelcontextprotocol/inspector tsx src/server.ts
```

Opens `http://localhost:5173` — browse tools, invoke them with test arguments, inspect JSON-RPC.

---

## Production path

The TypeScript proxy is the starting point. The production path is `Querify.MCP.Server` — a
native .NET project inside `Querify.sln` that calls MediatR handlers directly, serves all five
module agent types (QnA, Direct, Broadcast, Trust, Tenant), and participates in the same database
transaction as the rest of the backend.

See the full design: [`../future/integrations/mcp.md`](../future/integrations/mcp.md)

Source → Q&A generation pipeline: [`../future/integrations/mcp-source-to-qna.md`](../future/integrations/mcp-source-to-qna.md)
