FROM mcr.microsoft.com/dotnet/sdk:8.0

WORKDIR /app

ARG UID
ARG GID

USER ${UID}:${GID}

CMD ["dotnet", "watch", "--no-launch-profile", "--project", "src/Api/Kapoot.Api.csproj", "run"]
