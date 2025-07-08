# FintaCharts API Consumer

An ASP.NET Core Miminal APIs backend for storing FintaCharts data in QuestDB
and caching real-time socket data.


## Run

Define environment variables, for instance, in an `.env` file:
```
BASE_URL=https://your-host.com
AUTH_REALM=abcd
AUTH_USERNAME=myuser
AUTH_PASSWORD=mypassword
WSS_URI=wss://your-host.com
QUESTDB_CONNECTION_STRING='http::addr=127.0.0.1:9000;username=admin;password=quest;'
QUESTDB_PROTOCOL=http
QUESTDB_URL=localhost:9000
QUESTDB_USERNAME=admin
QUESTDB_PASSWORD=quest
CORS_FRONTEND=http://localhost:3000
VITE_BACKEND=http://localhost:5000
```

Run the apps via Docker
```
docker compose up --build
```

Browse backend REST endpoints on `http://localhost:5000/swagger`
Browse frontend client on `http://localhost:3000` and `http://localhost:3000/symbol/ABC`

Run the backend manually
```
cd backend-dotnet
dotnet restore
dotnet build -c Release -o out
dotnet ./out/backend-dotnet.dll
```


Run the frontend manually
```
cd frontend-solid
npm run build
npm run preview
```
