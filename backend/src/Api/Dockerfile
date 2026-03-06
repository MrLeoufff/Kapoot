# Build
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["Api/Kapoot.Api.csproj", "Api/"]
COPY ["Application/Kapoot.Application.csproj", "Application/"]
COPY ["Domain/Kapoot.Domain.csproj", "Domain/"]
COPY ["Infrastructure/Kapoot.Infrastructure.csproj", "Infrastructure/"]

RUN dotnet restore "Api/Kapoot.Api.csproj"
COPY . .
RUN dotnet build "Api/Kapoot.Api.csproj" -c Release -o /app/build

# Publish
FROM build AS publish
RUN dotnet publish "Api/Kapoot.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Run
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS final
WORKDIR /app
EXPOSE 80
ENV ASPNETCORE_URLS=http://+:80
COPY --from=publish /app/publish .
# Chaîne de connexion fournie par docker-compose (MySQL) ou défaut SQLite
ENTRYPOINT ["dotnet", "Kapoot.Api.dll"]
