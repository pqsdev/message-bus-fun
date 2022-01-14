#See https://aka.ms/containerfastmode to understand how Visual Studio uses this Dockerfile to build your images for faster debugging.

FROM mcr.microsoft.com/dotnet/aspnet:5.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:5.0 AS build
WORKDIR /src
COPY ["PQS.MessageBusFun.Api/PQS.MessageBusFun.Api.csproj", "PQS.MessageBusFun.Api/"]
COPY ["PQS.MessageBusFun.Messaging.Contracts/PQS.MessageBusFun.Messaging.Contracts.csproj", "PQS.MessageBusFun.Messaging.Contracts/"]
RUN dotnet restore "PQS.MessageBusFun.Api/PQS.MessageBusFun.Api.csproj"
COPY . .
WORKDIR "/src/PQS.MessageBusFun.Api"
RUN dotnet build "PQS.MessageBusFun.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "PQS.MessageBusFun.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "PQS.MessageBusFun.Api.dll"]