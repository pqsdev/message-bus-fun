# PQS Message Bus Fun
Sample dotnet 5 project using Docker, RabbitMQ, Masstransit and SQL Server

Proyects
- API : Rest API to invoke via swagger ui
- Worker: Consumers are hosted here, you can generate as meny instances as you like

Docker-compose
- SQL Server : useD to persist Job state 
- RabitMQ: Message broker
- API
- Worker



## BUILD

```powershell
cd deployment
docker-compose build --no-cache
```

# RUN
Build first

### Using docker compose 

```powershell
cd deployment
docker-compose up -d
```

### Using kubernetes
Needs
- sql server running 
- RabbitMQ cluster 
- Modify `appsettings.json` in `message-bus-fun-api-config` ConfigMap 

```powershell
kubectl apply -f .\deployment.yml

```
