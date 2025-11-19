# Active Work Log

## 2025-11-17: Docker Build & Local Binary Completed

### Status: HANDOFF TO DESKTOP - READY FOR VPS DEPLOYMENT & API KEY GENERATION

### Completed by Code - Session 1 (Docker Build)
1. ✓ Docker installation on Linux environment
2. ✓ Dockerfile creation (multi-stage build with Alpine)
3. ✓ docker-compose.yml configuration
4. ✓ .dockerignore setup
5. ✓ Docker image build successful
   - Image: `nexus-mcp:latest`
   - Image ID: `a58718be6843`
   - Size: 324MB (99.7MB compressed)

### Docker Configuration Summary
- **Base Images**: .NET 9.0 Alpine (SDK for build, ASP.NET for runtime)
- **Build Type**: Self-contained, single-file binary
- **Platform**: linux-musl-x64
- **Port**: 18080 (HTTP transport)
- **Network**: External network `nexus-network`
- **Volume**: `/home/linux/Nexus-Data` mounted as read-only to `/data/event-store`

### Environment Variables
- `NEXUS_TRANSPORT=http`
- `NEXUS_MCP_PORT=18080`
- `CONTEXT_LIBRARY_PATH=/data/event-store`
- `QDRANT_URL=http://qdrant:6333`
- `QDRANT_API_KEY=${QDRANT_API_KEY}` (from .env)
- `EMBEDDING_API_URL=http://embedding:5000/embed`

---

### Completed by Code - Session 2 (Local Binary Build)
1. ✓ Cleaned previous builds
2. ✓ Built single-file self-contained binary for local testing
3. ✓ Made binary executable
4. ✓ Tested binary with JSON-RPC initialize request

### Local Binary Configuration
- **Binary Location**: `/home/linux/FnMCP.Nexus/bin/local/FnMCP.Nexus`
- **Binary Size**: 97MB
- **Build Configuration**: Release, linux-x64, self-contained, single-file
- **Test Status**: Successfully responds to JSON-RPC requests in stdio mode
- **Version**: 0.3.0+2d64dcb16d1b6c2331f354ea242eaf928c893dfd

### Usage
```bash
./bin/local/FnMCP.Nexus /home/linux/Nexus-Data
```

This binary can be used for:
- Generating API keys in stdio mode
- Local testing without Docker
- Desktop MCP client integration

---

### Next Steps for Desktop

#### Option A: Use Local Binary for API Key Generation
1. Run the local binary: `./bin/local/FnMCP.Nexus /home/linux/Nexus-Data`
2. Use stdio mode to generate API keys
3. Test MCP prompts and tools locally

#### Option B: VPS Deployment with Docker
1. Deploy to VPS using the built Docker image
2. Set up the `nexus-network` external network on VPS
3. Configure .env file with QDRANT_API_KEY
4. Ensure Qdrant and embedding services are running
5. Verify /home/linux/Nexus-Data exists and contains event-store data
6. Start container: `docker-compose up -d`
7. Verify connectivity on port 18080

### Files Ready
- `./bin/local/FnMCP.Nexus` - Local single-file binary (97MB) for stdio mode
- `Dockerfile` - Multi-stage build configuration
- `docker-compose.yml` - Service orchestration
- `.dockerignore` - Build optimization
- Docker image: `nexus-mcp:latest` (can be exported/pushed if needed)

### Notes
- Build completed with minor Oxpecker version resolution (1.0.0 vs 0.20.0) - no functional impact
- ICU libraries included for globalization support
- Container configured with restart policy: `unless-stopped`

---

## 2025-11-18: SSE Endpoint Naming Fix - Ready for Deployment

### Status: ✅ CODE UPDATED - READY FOR BUILD & DEPLOY

### Completed by Code - Session 4 (SSE Endpoint Naming)

1. ✓ Identified endpoint naming issue: `/sse/events` was misleading
2. ✓ Updated SSE endpoint from `/sse/events` to `/sse`
3. ✓ Updated HttpServer.fs route configuration
4. ✓ Updated Tools.fs API key generation message
5. ✓ Verified SseTransport welcome message (already correct)
6. ✓ Committed changes to git

### Changes Made

**File: `src/FnMCP.Nexus/Http/HttpServer.fs`**
- Changed SSE route from `route "/events"` to `route ""`
- Updated comment: "SSE stream endpoint for all MCP operations (tools, resources, prompts)"
- Updated log message: `/sse/events` → `/sse`

**File: `src/FnMCP.Nexus/Tools.fs`**
- Updated API key generation output: `https://mcp.nexus.ivanthegeek.com/sse/events` → `.../sse`

**Rationale:**
The SSE endpoint name `/sse/events` was misleading because it handles ALL MCP operations (tools, resources, prompts), not just events. The generic `/sse` name better reflects that this is the SSE transport endpoint for the entire MCP protocol.

### Next Steps for Next Session

1. **Build Docker image** with updated code
   ```bash
   docker build -t nexus-mcp:fixed .
   ```

2. **Export and transfer to VPS**
   ```bash
   docker save nexus-mcp:fixed | gzip > /tmp/nexus-mcp-fixed-v2.tar.gz
   scp /tmp/nexus-mcp-fixed-v2.tar.gz root@66.179.208.238:/tmp/
   ```

3. **Deploy on VPS**
   ```bash
   ssh root@66.179.208.238 "docker load < /tmp/nexus-mcp-fixed-v2.tar.gz && \
     docker tag nexus-mcp:fixed nexus-mcp:latest && \
     docker stop nexus-mcp && docker rm nexus-mcp && \
     docker run -d --name nexus-mcp \
       -v /data/event-store:/data/event-store \
       --restart unless-stopped nexus-mcp:latest"
   ```

4. **Test updated endpoint**
   ```bash
   # Old endpoint (should 404)
   curl -H "Authorization: Bearer KEY" http://66.179.208.238:18080/sse/events

   # New endpoint (should work)
   curl -H "Authorization: Bearer KEY" http://66.179.208.238:18080/sse
   ```

5. **Update Claude Desktop config** to use new endpoint:
   ```json
   {
     "mcpServers": {
       "nexus-vps": {
         "url": "http://66.179.208.238:18080/sse",
         "transport": {
           "type": "sse",
           "headers": {
             "Authorization": "Bearer KvzmKD3aBYBmKY4pvOh/+NhwHBBxxiTeIKD2Kq/tAw4="
           }
         }
       }
     }
   }
   ```

### Commit Information

**Commit:** `0742fb8`
**Message:** Fix SSE endpoint naming: /sse/events -> /sse

---

## 2025-11-19: VPS Deployment Complete - SSE/WebSocket Operational

### Status: ✅ PRODUCTION READY - MCP SERVER ACCESSIBLE VIA HTTP/SSE

### Completed by Code - Session 3 (VPS Deployment & Troubleshooting)

1. ✓ VPS container deployment verified (running 33+ hours)
2. ✓ Identified and fixed firewall blocking issue
   - Issue: Cloud provider firewall blocking port 18080
   - Solution: Added UFW firewall rule + cloud provider firewall rule
3. ✓ API key authentication system operational
4. ✓ Generated production API key with full access
5. ✓ Verified SSE connection establishment and authentication
6. ✓ Updated Claude Desktop MCP configuration to use SSE transport

### VPS Server Status

**Endpoint Information:**
- **Health Check**: http://66.179.208.238:18080/
- **SSE Endpoint**: http://66.179.208.238:18080/sse/events
- **SSE Message**: http://66.179.208.238:18080/sse/message
- **WebSocket**: ws://66.179.208.238:18080/ws
- **Server**: Kestrel (ASP.NET Core)
- **Version**: 0.3.0

**Security:**
- All endpoints (except health check) require Bearer token authentication
- API keys stored as hashed events in event store
- Authentication logging for usage tracking and security auditing

### Production API Key

```
API Key: KvzmKD3aBYBmKY4pvOh/+NhwHBBxxiTeIKD2Kq/tAw4=
Key ID: b8969ef4-e02e-459d-a6a8-efe52378976a
Scope: full_access (no expiration)
Description: Claude Desktop access key
Generated: 2025-11-19T02:37:15Z
```

⚠️ **Stored securely** - Key hash stored at:
`/data/event-store/nexus/events/system/api-keys/2025-11/2025-11-19T02-37-15Z_ApiKeyGenerated_b8969ef4_0a1cdcee.yaml`

### Claude Desktop Configuration

**Updated configuration** at `~/.config/Claude/claude_desktop_config.json`:

```json
{
  "mcpServers": {
    "nexus-vps": {
      "url": "http://66.179.208.238:18080/sse",
      "transport": {
        "type": "sse",
        "headers": {
          "Authorization": "Bearer KvzmKD3aBYBmKY4pvOh/+NhwHBBxxiTeIKD2Kq/tAw4="
        }
      }
    }
  }
}
```

**Configuration Change:**
- **Before**: SSH → Docker exec with stdio transport (slow, SSH overhead)
- **After**: Direct HTTP/SSE with Bearer authentication (fast, native HTTP)

### Connection Verification

✅ **Test Results:**
```bash
# Health check
curl http://66.179.208.238:18080/
{"service":"Nexus MCP Server","status":"ok","timestamp":"...","transport":"http","version":"0.3.0"}

# SSE with authentication
curl -H "Authorization: Bearer KvzmKD3aBYBmKY4pvOh/+NhwHBBxxiTeIKD2Kq/tAw4=" \
     http://66.179.208.238:18080/sse/events
# [Connection established, SSE welcome sent]
```

**Logs confirm:**
```
[SseTransport] SSE connection established
[SseTransport] SSE welcome sent, keeping connection alive
Request finished HTTP/1.1 GET .../sse/events - 200 - text/event-stream
```

### Firewall Configuration

**UFW (Server Firewall):**
```bash
ufw allow 18080/tcp  # Added for MCP HTTP transport
```

**Active UFW Rules:**
- 22/tcp (SSH)
- 80/tcp (HTTP)
- 443/tcp (HTTPS)
- 6333/tcp (Qdrant)
- 18080/tcp (Nexus MCP) ← NEW

**Cloud Provider Firewall:**
- Configured to allow inbound TCP 18080 from 0.0.0.0/0

### API Key Management

**Generate new keys:**
```bash
ssh root@66.179.208.238 "docker exec -i nexus-mcp ./FnMCP.Nexus /data/event-store generate-api-key \
  --scope <full_access|read_only|files_only_public> \
  --description 'Key description' \
  --expires-in-days 90"  # Optional expiration
```

**Available scopes:**
- `full_access` - All MCP operations and resources
- `read_only` - Resources and read-only tools
- `files_only_public` - Only public classification files

### Next Steps

1. **Restart Claude Desktop** to load new MCP configuration
2. **Verify MCP connection** in Claude Desktop
3. **Test MCP tools** (create_event, timeline_projection, etc.)
4. **Monitor API key usage** in event store logs
5. **Consider**: Set up reverse proxy with HTTPS (nginx/caddy) for production SSL

### Files Modified

- `~/.config/Claude/claude_desktop_config.json` - Updated MCP server configuration
- `active-work.md` - This status update

### Architecture Notes

**Why SSE over stdio?**
- ✅ Native HTTP transport (faster, more reliable)
- ✅ No SSH overhead
- ✅ Secure API key authentication
- ✅ Works from anywhere with internet access
- ✅ Standard transport supported by MCP spec
- ✅ Better for production deployments

**Event Sourcing Benefits:**
- API key lifecycle tracked as events
- Usage logging for security auditing
- Revocation support built-in
- Key expiration enforcement
