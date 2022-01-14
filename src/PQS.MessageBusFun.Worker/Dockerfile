#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["PQS.MessageBusFun.Worker/PQS.MessageBusFun.Worker.csproj", "PQS.MessageBusFun.Worker/"]
COPY ["PQS.MessageBusFun.Messaging.Components/PQS.MessageBusFun.Messaging.Components.csproj", "PQS.MessageBusFun.Messaging.Components/"]
COPY ["PQS.MessageBusFun.Messaging.Contracts/PQS.MessageBusFun.Messaging.Contracts.csproj", "PQS.MessageBusFun.Messaging.Contracts/"]
RUN dotnet restore "PQS.MessageBusFun.Worker/PQS.MessageBusFun.Worker.csproj"
COPY . .
WORKDIR "/src/PQS.MessageBusFun.Worker"
RUN dotnet build "PQS.MessageBusFun.Worker.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PQS.MessageBusFun.Worker.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PQS.MessageBusFun.Worker.dll"]