# Docker Network

This enables data transmission between containers.
---
# Steps To Implement:

# ## Network
- **Create a Network**
```
docker network create training-network    
```
- **List all Networks**
```
docker network ls
```
# ## Database
- **Install db image from docker hub registry**
```
docker pull postgres:latest
```
- **Create container**
```
docker run -d \
  --name pg \
  --network training-network \
  -p 5433:5432
  -e POSTGRES_USER=postgres \
  -e POSTGRES_PASSWORD=secret \
  -e POSTGRES_DB=BankingAPI \
  postgres

```
- -d : detach mode (runs in background)
- --name pg : sets the name of the container to pg
- --network : register the container to training-network
- netowrk port. 5433 when connection using localhost. 5432 when connection between containers through docker network
- -e sets username, password and db. (db is automatically created)
- postgres at the end refer to the image type.

---

# ## Backend
- Do these steps in the backend directory terminal
- **Migrate Data**
1. Change the connection string to
```
Host=localhost; Port=5433, username and password accordingly.
```
This is done to migrate data using local network to pg container. Backend is system to pg container communication.
2. data migration
```
dotnet ef database update
```
- **Create Image**
1. Change Connection String
```
Host=pg; Port=5432, username and password accordingly.
```
Now container to container communication

2. Image:
```
docker build -t banking-api:1.0 .
```
- -t => tag for image version
- banking-api:1.0 => image name
- . => current directory


3. Container:
```
docker run -d \        
--name banking-api \
--network training-network \
-p 8080:8080 \
banking-api:1.0
```
---
# ## Verification
1. Check contaiers are running
```
docker ps
```
if not, run start them:
```
docker start container_name
```
2. If any errors occured, check using:
```
docker logs -f container_name
```
- -f => follow up (shows updated log while working)

3. Network Inspect
```
docker network inspect network_name
```
- this show both the containers.

# ## Output
- Since both backend and db containers are running. open swagger and try executing the endpoints
```
http://localhost:8080/swagger/index.html
```

# ## DB
- Db can be viewed in docker by 2 ways
1. Interactive shell
```
docker exec -it pg sh
psql -U postgres
\l => list db
\c db => chooses a db
\dt => lists tables
select * from public."tablename"
```

2. DBeaver
Create new connection by giving local host details
```
Host = localhost
Port = 5433
username, password and db
```
