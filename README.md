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
cp client/.env.example client/.env
dotnet user-secrets set "ConnectionStrings:Default" "Host=localhost;Port=5432;Database=boardgameutils;Username=postgres;Password=YOUR_PASSWORD" --project server
```

Replace `YOUR_PASSWORD` with your PostgreSQL password. The database, its tables and the sample data are created automatically when the server starts.

## Run

```
npm start
```

The client opens at http://localhost:5173.

## Tests

```
npm test
```

## Configuration

| Setting | Where | Meaning |
|---|---|---|
| `ConnectionStrings:Default` | .NET user secrets | PostgreSQL connection |
| `VITE_TENANT_ID` | `client/.env` | The organisation the client acts as (`1` or `2`) |

## Sample data

| Organisation | Tenant ID | Room code | Players |
|---|---|---|---|
| Organisation 1 | 1 | `AAAAAA` | Player1, Player2 |
| Organisation 2 | 2 | `BBBBBB` | Player3, Player4 |

## Demo

### Main scenario

1. Open http://localhost:5173, enter a name and a max player count from 2 to 5, then click **Create Room**.
2. Copy the room code from the end of the URL.
3. Open http://localhost:5173 in a second tab, enter another name and the code, then click **Join Room**. Both players appear in both tabs.
4. In the first tab, click **Start Game**. Each tab shows two objectives.
5. Click one objective in each tab. The chosen objective appears next to the player's name.

### Errors and conflicts

1. In a third tab, try to join the same room. You get "Game has already started".
2. Create a new room and click **Start Game** while alone in it. You get "Not enough players."

### Isolation between organisations

1. With `VITE_TENANT_ID=1`, join room `BBBBBB`. You get "Room not found", because it belongs to Organisation 2.
2. Join room `AAAAAA`. This works.
3. Stop the app, set `VITE_TENANT_ID=2` in `client/.env`, and run `npm start` again.
4. Join room `AAAAAA`. You get "Room not found".
5. Join room `BBBBBB`. This works.
