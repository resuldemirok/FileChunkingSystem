# Build stage
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

COPY ["FileChunkingSystem.Console/FileChunkingSystem.Console.csproj", "FileChunkingSystem.Console/"]
COPY ["FileChunkingSystem.Application/FileChunkingSystem.Application.csproj", "FileChunkingSystem.Application/"]
COPY ["FileChunkingSystem.Application.Tests/FileChunkingSystem.Application.Tests.csproj", "FileChunkingSystem.Application.Tests/"]
COPY ["FileChunkingSystem.Domain/FileChunkingSystem.Domain.csproj", "FileChunkingSystem.Domain/"]
COPY ["FileChunkingSystem.Infrastructure/FileChunkingSystem.Infrastructure.csproj", "FileChunkingSystem.Infrastructure/"]

RUN dotnet restore "FileChunkingSystem.Console/FileChunkingSystem.Console.csproj"

COPY . .

# Test stage
FROM build AS test
WORKDIR /src/FileChunkingSystem.Application.Tests
RUN dotnet test --no-restore --verbosity normal

# Publish stage
FROM build AS publish
WORKDIR /src/FileChunkingSystem.Console
RUN dotnet publish "FileChunkingSystem.Console.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Runtime stage
FROM mcr.microsoft.com/dotnet/runtime:9.0 AS final
WORKDIR /app
RUN mkdir -p /app/chunks /app/logs /app/files /app/restored
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "FileChunkingSystem.Console.dll"]
