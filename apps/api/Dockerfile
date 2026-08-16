# Build context: monorepo root
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ModelaFlow.sln ./
COPY apps/api/ModelaFlow.Api.csproj apps/api/
COPY packages/pattern-core/ModelaFlow.PatternCore.csproj packages/pattern-core/
COPY packages/pattern-export/ModelaFlow.PatternExport.csproj packages/pattern-export/

RUN dotnet restore apps/api/ModelaFlow.Api.csproj

COPY apps/api/ apps/api/
COPY packages/pattern-core/ packages/pattern-core/
COPY packages/pattern-export/ packages/pattern-export/

RUN dotnet publish apps/api/ModelaFlow.Api.csproj -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl \
    && rm -rf /var/lib/apt/lists/*
COPY --from=build /app/publish .
ENV ASPNETCORE_URLS=http://+:8080
EXPOSE 8080
HEALTHCHECK --interval=10s --timeout=5s --start-period=20s --retries=5 \
  CMD curl -fsS http://127.0.0.1:8080/health || exit 1
ENTRYPOINT ["dotnet", "ModelaFlow.Api.dll"]
