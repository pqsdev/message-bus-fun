# BUILD

```powershell
docker-compose build --no-cache
```

# RUN
Build first

### Using docker compose 

```powershell
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
