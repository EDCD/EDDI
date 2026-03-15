# EDDI ↔ VoiceAttack Plugin IPC Messaging Protocol

## Overview
Cross-platform IPC using TCP sockets with JSON message format. Enables bidirectional communication between EDDI core engine and VoiceAttack plugin.

---

## Architecture

### IPC Port Discovery

EDDI automatically selects the first available TCP port starting from 12345 (port range: 12345-12450). The selected port is written to `ipc_config.json` for VoiceAttack plugin discovery.

#### Port Selection Process
1. EDDI searches for available port starting from 12345
2. If 12345 is unavailable, tries 12346, 12347, etc.
3. Binds IPC server to first available port
4. Writes actual port to `ipc_config.json`
5. Logs selected port for diagnostics

#### VA Plugin Port Discovery
At VA_Init1, the plugin:
1. Reads `ipc_config.json` from EDDI config directory
2. Extracts `port` value
3. Connects to `127.0.0.1:[port]`

**File**: `ipc_config.json`
```json
{
  "port": 12345
}
```

### Endpoints

#### 1. **EDDI Server**
- **Role**: Listens for incoming connections from VA plugin
- **Lifecycle**: Started when EDDI initializes; stopped when EDDI stops
- **Address**: `127.0.0.1:[auto-selected port]` (IPv4 loopback for security)
- **Port Range**: 12345-12450 (auto-selected, first available)
- **Bidirectional Communication**:
  - **Send**: Events, state synchronization, command responses → to VA plugin
  - **Receive**: Commands, acknowledgments, heartbeats ← from VA plugin

#### 2. **VoiceAttack Plugin Client**
- **Role**: Initiates connection to EDDI server on VA startup
- **Lifecycle**: Connected during VA_Init1; disconnected during VA_Exit1
- **Port Discovery**: Reads `ipc_config.json` to find server port
- **Retry Logic**: Exponential backoff with max 10 retries on connection failure
- **Bidirectional Communication**:
  - **Send**: Commands, acknowledgments, heartbeats → to EDDI server
  - **Receive**: Events, state synchronization, command responses ← from EDDI server

---

## Message Format

All messages are **JSON-encoded** with a **header + payload** structure:

```
[LENGTH]\n[PAYLOAD]
```

### Header
- **LENGTH**: UTF-8 encoded byte count of the JSON payload (unsigned int, followed by newline `\n`)
- **Example**: `147\n{"type":"Heartbeat",...}`

### Payload Structure
```json
{
  "type": "MessageType",
  "timestamp": "ISO8601 UTC timestamp",
  "id": "unique-message-id-uuid",
  "data": { /* message-specific payload */ }
}
```

### Common Fields
- **type**: Message classification (see Message Types below)
- **timestamp**: ISO 8601 UTC (e.g., `2025-01-20T15:30:45.123Z`)
- **id**: UUID for request/response matching and deduplication
- **data**: Message-specific payload

---

## Message Types

### 1. **Heartbeat Messages** (Keep-Alive)

#### `Heartbeat` (Bidirectional)
**Direction**: EDDI → VA Plugin or VA Plugin → EDDI  
**Frequency**: Every 5 seconds  
**Timeout**: No response after 10 seconds = connection dead

```json
{
  "type": "Heartbeat",
  "timestamp": "2025-01-20T15:30:45.123Z",
  "id": "550e8400-e29b-41d4-a716-446655440000",
  "data": {
    "status": "alive",
    "uptime_ms": 123456
  }
}
```

**Response**: Echo back with same `id`

---

### 2. **Connection Lifecycle**

#### `Connect` (VA Plugin → EDDI)
**Purpose**: Establish connection, exchange capabilities

```json
{
  "type": "Connect",
  "timestamp": "2025-01-20T15:30:45.123Z",
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "data": {
    "plugin_version": "5.0.0",
    "plugin_name": "EDDI VoiceAttack Plugin",
    "capabilities": ["events", "commands"],
    "supported_message_types": ["Connect", "Heartbeat", "Disconnect", "Event", "Command"]
  }
}
```

#### `ConnectAck` (EDDI → VA Plugin)
**Purpose**: Accept connection, confirm capabilities

```json
{
  "type": "ConnectAck",
  "timestamp": "2025-01-20T15:30:45.124Z",
  "id": "550e8400-e29b-41d4-a716-446655440001",
  "data": {
    "eddi_version": "5.0.0",
    "server_name": "EDDI Core Engine",
    "accepted": true,
    "capabilities": ["events", "commands"],
    "supported_message_types": ["Connect", "Heartbeat", "Disconnect", "Event", "Command"],
    "session_id": "550e8400-e29b-41d4-a716-446655440099"
  }
}
```

#### `Disconnect` (Either direction)
**Purpose**: Gracefully close connection

```json
{
  "type": "Disconnect",
  "timestamp": "2025-01-20T15:30:45.125Z",
  "id": "550e8400-e29b-41d4-a716-446655440002",
  "data": {
    "reason": "user_shutdown | network_error | heartbeat_timeout",
    "message": "User initiated shutdown"
  }
}
```

---

### 3. **Event Messages**

#### `Event` (EDDI → VA Plugin)
**Purpose**: Notify plugin of EDDI events (e.g., location change, docked, jumped)

```json
{
  "type": "Event",
  "timestamp": "2025-01-20T15:30:45.126Z",
  "id": "550e8400-e29b-41d4-a716-446655440003",
  "data": {
    "event_type": "LocationChanged",
    "event_name": "Location",
    "system": "Sol",
    "station": "Starport",
    "body": "Earth",
    "raw_event": { /* full event object */ }
  }
}
```

---

### 4. **Command Messages**

#### `Command` (VA Plugin → EDDI)
**Purpose**: Send commands to EDDI (e.g., enable/disable monitor, trigger action)

```json
{
  "type": "Command",
  "timestamp": "2025-01-20T15:30:45.128Z",
  "id": "550e8400-e29b-41d4-a716-446655440004",
  "data": {
    "command": "enable_monitor | disable_monitor | trigger_responder",
    "target": "Journal Monitor",
    "parameters": {}
  }
}
```

#### `CommandResponse` (EDDI → VA Plugin)
**Purpose**: Respond to command with result

```json
{
  "type": "CommandResponse",
  "timestamp": "2025-01-20T15:30:45.129Z",
  "id": "550e8400-e29b-41d4-a716-446655440004",
  "data": {
    "command_id": "550e8400-e29b-41d4-a716-446655440004",
    "status": "success | error",
    "message": "Monitor enabled successfully",
    "result": {}
  }
}
```

---

### 5. **Query Messages** (On-Demand State Requests)

#### `Query` (VA Plugin → EDDI)
**Purpose**: Request specific state or data from EDDI (on-demand)

```json
{
  "type": "Query",
  "timestamp": "2025-01-20T15:30:45.130Z",
  "id": "550e8400-e29b-41d4-a716-446655440005",
  "data": {
    "query_type": "GetCurrentState",
    "parameters": {
      "include_history": true
    }
  }
}
```

**Supported Query Types:**
- `GetCurrentState`: Returns full EDDI state (commander, ship, location, etc.)
- `GetJournalEntries`: Returns recent journal entries
- `GetVehicleStatus`: Returns current vehicle status

#### `QueryResponse` (EDDI → VA Plugin)
**Purpose**: Respond to query with requested data

```json
{
  "type": "QueryResponse",
  "timestamp": "2025-01-20T15:30:45.131Z",
  "id": "550e8400-e29b-41d4-a716-446655440005",
  "data": {
    "query_id": "550e8400-e29b-41d4-a716-446655440005",
    "status": "success | error",
    "result": {
      "commander_name": "CMDR TestCommander",
      "current_system": "Sol",
      "current_station": "Starport",
      "ship_name": "Test Ship"
    },
    "message": "Query executed successfully"
  }
}
```

---

### 6. **Error Messages**

#### `Error` (Either direction)
**Purpose**: Report errors in message processing

```json
{
  "type": "Error",
  "timestamp": "2025-01-20T15:30:45.132Z",
  "id": "550e8400-e29b-41d4-a716-446655440006",
  "data": {
    "error_code": "INVALID_MESSAGE_FORMAT | UNKNOWN_MESSAGE_TYPE | PROCESSING_ERROR",
    "message": "Invalid JSON in payload",
    "original_message_id": "550e8400-e29b-41d4-a716-446655440003",
    "details": {}
  }
}
```

---

## Connection Lifecycle

### 1. **EDDI Startup**
```
EDDI.Instance initialized
  ↓
EDDI selects available port (12345, 12346, ...)
  ↓
EDDI starts TCP server on selected port
  ↓
EDDI writes selected port to ipc_config.json
  ↓
Server ready to accept connections
  ↓
Log: "IPC Server will listen on port 12345"
Log: "IPC configuration written to [...]/ipc_config.json (port: 12345)"
```

### 2. **VoiceAttack Plugin Initialization** (VA_Init1)
```
VA_Init1() called
  ↓
Plugin reads ipc_config.json
  ↓
Plugin extracts port from JSON
  ↓
Plugin attempts connection to 127.0.0.1:[port]
  ↓
On success:
  - Send Connect message
  - Receive ConnectAck
  - Store session_id in SessionState["eddi_session_id"]
  ↓
On failure:
  - Retry with exponential backoff (1s, 2s, 4s, 8s, 16s, ...)
  - Max 10 retries, then log warning and continue (VA can work offline)
```

### 3. **Normal Operation** (Event-Driven + On-Demand Queries)
```
Both sides send Heartbeat every few seconds
  ↓
Event occurs in EDDI
  ↓
EDDI sends Event message to VA plugin (push-based)
  ↓
VA plugin receives Event and processes immediately (fire-and-forget)
  ↓
When VA plugin needs current state:
  - Send Query message (e.g., GetCurrentState)
  - Receive QueryResponse from EDDI
  - Update local state from response
  ↓
No continuous state synchronization (on-demand instead)
```

**Benefits of this pattern:**
- Low latency: Events pushed immediately, no polling needed
- Minimal overhead: StateSync eliminated, only query when needed
- Scalable: Plugin controls state refresh frequency
- Maintainable: Clear separation of push (events) vs. pull (queries)

### 4. **Connection Loss Detection**
```
No Heartbeat received for 10 seconds
  ↓
Attempt to send Heartbeat to verify connection
  ↓
If no response after to Heartbeat:
  - Mark connection dead
  - Begin reconnection attempts
  - Notify other components (logging, UI)
```

### 5. **EDDI Shutdown**
```
EDDI.Instance.Stop() called
  ↓
Send Disconnect message to connected plugins
  ↓
Close TCP server
  ↓
Await graceful client disconnections (5 second timeout)
  ↓
Force close remaining connections
```

### 6. **VoiceAttack Plugin Shutdown** (VA_Exit1)
```
VA_Exit1() called
  ↓
Send Disconnect message to EDDI
  ↓
Close socket
  ↓
Clear SessionState["eddi_session_id"]
```

---

## VoiceAttack SessionState Keys

```csharp
// Connection Management
SessionState["eddi_port"] = 12345                        // EDDI server port
SessionState["eddi_session_id"] = "UUID"                 // Current session ID from ConnectAck
SessionState["eddi_connected"] = true/false              // Connection status
SessionState["eddi_last_heartbeat"] = DateTime.UtcNow    // Last heartbeat timestamp

// Application State (synced from EDDI)
SessionState["eddi_commander"] = "CMDR Name"
SessionState["eddi_system"] = "Sol"
SessionState["eddi_station"] = "Starport"
SessionState["eddi_ship"] = "Ship Name"
SessionState["eddi_environment"] = "Docked"
SessionState["eddi_vehicle"] = "Ship"

// Event Deduplication
SessionState["eddi_last_event_id"] = "UUID"              // Last processed event ID
SessionState["eddi_event_buffer"] = new List<string>     // Recent event IDs

// Retry/Backoff Management
SessionState["eddi_reconnect_attempts"] = 0
SessionState["eddi_last_reconnect_time"] = DateTime.UtcNow
```

---

## Implementation Timeline

### Phase 1: Core Infrastructure
- [ ] IPC Server base class (TcpListener-based)
- [ ] Message serialization/deserialization
- [ ] Heartbeat mechanism
- [ ] Connection lifecycle management

### Phase 2: EDDI Integration
- [ ] IPC Server in EDDI.cs
- [ ] Event broadcasting to connected clients
- [ ] State synchronization
- [ ] Graceful shutdown

### Phase 3: VA Plugin Integration
- [ ] IPC Client in VA plugin
- [ ] SessionState management
- [ ] Connection recovery with backoff
- [ ] Event processing

### Phase 4: Testing & Documentation
- [ ] Unit tests for message serialization
- [ ] Integration tests for connection lifecycle
- [ ] Load testing (rapid connections/disconnections)
- [ ] Cross-platform validation (Windows, Linux)

---

## Security Considerations

1. **Localhost Only**: Server binds to `127.0.0.1` (not `0.0.0.0`)
2. **No Authentication**: Trust is implicit (same machine)
3. **Message Validation**: All messages must have valid JSON and required fields
4. **Resource Limits**: Max 10 concurrent connections; rate-limit messages per connection

---

## Error Codes

| Code | Meaning | Recovery |
|------|---------|----------|
| `INVALID_MESSAGE_FORMAT` | Malformed JSON | Resend with correct format |
| `UNKNOWN_MESSAGE_TYPE` | Type not recognized | Update protocol version |
| `PROCESSING_ERROR` | EDDI error handling message | Check EDDI logs |
| `HEARTBEAT_TIMEOUT` | No heartbeat in 10s | Reconnect |
| `SESSION_MISMATCH` | Session ID doesn't match | Reconnect and get new session |

