FROM mcr.microsoft.com/dotnet/runtime:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["BackendServer.csproj", "."]
RUN dotnet restore "BackendServer.csproj"
COPY . .
RUN dotnet build "BackendServer.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "BackendServer.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "BackendServer.dll"]
