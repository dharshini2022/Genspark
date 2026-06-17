# Volumes
- These are storage backups that are used to persist the container data beyound the lifecyle of the container.
- If a container is deleted, the data associated with it also gets deleted.
- But by using volumes, a copy of the data is stored externally as a back up.

---

# Types of Volumes
- **DOCKER MANAGED VOLUME:**
- The data is stored and manged by docker.
- Iscolated from host OS
- Used in Production

- **BIND MOUNTS:**
- stored within a directory of the local system
- managed by host os
- Used during Development

---

# Implementation
# ## Docker Managed Volumes
- **Create volume**
```
docker volume create volume_name
```

- **Create a container associated to the volume**
```
docker run -d \
--name postgres-db \
-v postgres-data:/var/lib/postgresql/data \
-e POSTGRES_PASSWORD=postgres \
-p 5432:5432 \
postgres:16
```
- -v postgres-data (name of the volume = source):/var/lib/postgresql/data \  (data storage location within the container = destination)
- This works in 2 ways, 
1. Copies data from container at the specifed path to the volume
2. Restores data from the volume to the container

- **Delete Container:**
```
docker rm -f postgres-db
```
The container is deleted but the data is backed up successfully in the volume.

- **Create Another Container and Attach this Volume:**
```
docker run -d \
--name postgres-db2 \
-v postgres-data:/var/lib/postgresql/data \
-e POSTGRES_PASSWORD=postgres \
-p 5432:5432 \
postgres:16
```
- This container will automatically aquire the data from the volume
- How is merging handled automatically

- **CASE 1**
- The change data is not tracked by volume
- The change data is present in image1. Eg: EmailService in image1 uses sendgrid
- But the new container is from a different image, image2 where the EmailService uses SMTP.
- In that case the container aquires the change data from image
```
Change Data Not Tracked by Volume : IMAGE > VOLUME
```

- **CASE 2**
- The change data is tracked by the volume
- The change data is in the volume. 
- The the container aquires the data from the volume.
```
Change Data Tracked by Volume : Volume > Image
```

- **Inspect Volume**
```
docker volume inspect volume_name
```
- returns data like where the volume datas are stored

# ## BIND MOUNTS
- **Create a Local Directory to Store the Data:**
```
mkdir pgdata  
```

- **Copy Content to Local Volume:**
```
docker run -d \                    
--name pg-bind \
-v $(pwd)/pgdata:/var/lib/postgresql/data \
-e POSTGRES_PASSWORD=postgres \
postgres:16
```
- $(pwd) => Print Working Directory => Gives absolute path of pgdata

- **Inspect bind mount**
```
docker inspect bind_name
```


