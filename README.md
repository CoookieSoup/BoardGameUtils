# BoardGameUtils

## Requirements

| Tool | Version |
|---|---|
| .NET SDK | 10.0.401 |
| Node.js | 24.21.0 |
| PostgreSQL | 18 (must be running) |

## Setup

Run these once, from the `BoardgameUtils` folder:

```
cd BoardgameUtils
npm install
npm install --prefix client
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=boardgameutils;Username=postgres;Password=YOUR_PASSWORD" --project server
```

Replace `YOUR_PASSWORD` with your PostgreSQL password. The database, its tables and the sample data are created automatically when the server starts.

## Run

```
npm start
```

The client opens at http://localhost:5173.

## Run in Docker

Requires Docker Desktop. Once, from the `BoardgameUtils` folder:

```
cp .env.example .env
```

Set `POSTGRES_PASSWORD` in `.env`, then run:

```
docker compose up --build
```

The client opens at http://localhost:5173 and the API at http://localhost:5018. The containers read their configuration from `.env`:

| Variable | Used by | Meaning |
|---|---|---|
| `POSTGRES_USER`, `POSTGRES_PASSWORD` | db, server | Database login, passed to the server as `ConnectionStrings__Default` |
| `CLIENT_ORIGIN` | server | Allowed CORS origin (`ClientOrigin`) |
| `API_URL` | client | API address baked into the client build (`VITE_API_URL`) |

## Tests

```
npm test
```

## Configuration

| Setting | Where | Meaning |
|---|---|---|
| `ConnectionStrings:Default` | .NET user secrets | PostgreSQL connection |

Each room is its own organisation. When a player joins or creates a room, the server returns a token. The client sends it with every request, and the server uses it to decide which room's data the player can see.

## Sample data

| Organisation | Tenant ID | Room code | Password | Players |
|---|---|---|---|---|
| Room AAAAAA | 1 | `AAAAAA` | `apple` | Player1, Player2 |
| Room BBBBBB | 2 | `BBBBBB` | `banana` | Player3, Player4 |

## Demo

### Main scenario

1. Open http://localhost:5173, enter a name, a max player count from 2 to 5 and a room password, then click **Create Room**.
2. Copy the room code from the end of the URL.
3. Open http://localhost:5173 in a second tab, enter another name, the code and the password, then click **Join Room**. Both players appear in both tabs.
4. Write a message in the chat and click **Send**. It appears in both tabs.
5. In the first tab, click **Start Game**. Each tab shows two objectives.
6. Click one objective in each tab. The chosen objective appears next to the player's name.

### Errors and conflicts

1. In a third tab, try to join the same room. You get "Game has already started".
2. Create a new room and click **Start Game** while alone in it. You get "Not enough players."

### Isolation between organisations

Run these in Git Bash and replace `TOKEN` with the token from step 1.

1. Join room `AAAAAA` as a new player and keep the token from the response:

   ```
   curl -X POST http://localhost:5018/api/rooms/AAAAAA/join -H "Content-Type: application/json" -d "{\"name\":\"Tester\",\"password\":\"apple\"}"
   ```

2. Read your own room with that token. This works and lists the players:

   ```
   curl -H "Authorization: Bearer TOKEN" http://localhost:5018/api/rooms/AAAAAA/getPlayerInfo
   ```

3. Read room `BBBBBB` with the same token. You get `"Room not found"`, because it belongs to another organisation:

   ```
   curl -H "Authorization: Bearer TOKEN" http://localhost:5018/api/rooms/BBBBBB/getPlayerInfo
   ```
