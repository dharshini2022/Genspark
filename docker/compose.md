# DOCKER COMPOSE
- It is used to define, configure and manage services.
- Wihtout configure, we would be manually 
1. building the image
2. Create network 
3. Create Volume
4. instantiating container

```
docker network create mynetwork

docker run -d --name db --network mynetwork postgres:16

docker build -t my-backend .

docker run -d --name backend --network mynetwork my-backend

docker run -d --name frontend --network mynetwork nginx
```

---
# Service:
It combines all the operations into one file (docker-compose.yml)
```
services:
  db:
    image: postgres:16

  backend:
    build: .
    depends_on:
      - db

  frontend:
    image: nginx
    depends_on:
      - backend
```

---
# Service Commands
1. **Instantiate a Service:**
```
docker compose up -d --build service_name
```
- DB images have volume associated for data backup
- Build is for backend , frontend code
- when the application is live, users will make changes to db data (register, login) , so when the container is deleted, the db data also gets deleted.
- So we have volumes for db
- But users do not change the source code, volume is not necessary here. So the backend and frontend data remain in the image itself and not the volume
- So if we make changes to backend image, we must build the serive to reflect the changes on the newly created container

2. **Scalling**
```
docker compose up --scale db=3
```
- Creates 3 containers for db service

3. **Removing the containers**
```
docker compose down
```
- this deleted the containers and networks. Volumes persisted


```
docker compose down -v
```
- This also deleted the volumes along with containers and networks.

4. **Logs**
```
docker compose logs service_name
```

5. **Restart Service**
```
docker compose restart service_name
```
- All the containers associated with it will restart.
- There will be not data losses if the data is stored in volume.

6. **To Stop a Container:**
```
docker stop container_name
```
-container name allocation : folder of docker-compose.yml name-service-1

