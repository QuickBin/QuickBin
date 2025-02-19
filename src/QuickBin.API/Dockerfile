FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Backend build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS backend-build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["src/QuickBin.API/QuickBin.API.csproj", "QuickBin.API/"]
RUN dotnet restore "QuickBin.API/QuickBin.API.csproj"
COPY ./src .
WORKDIR "/src/QuickBin.API"
RUN dotnet build "QuickBin.API.csproj" -c $BUILD_CONFIGURATION -o /app/build

# Frontend build
FROM node:18 AS frontend-build
WORKDIR /src/frontend
RUN npm install -g @angular/cli@19
COPY src/QuickBin.Frontend/package*.json .
RUN npm ci
COPY src/QuickBin.Frontend .
RUN npm run build:agent --output-path /.artifacts/frontend

FROM backend-build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "QuickBin.API.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false
COPY --from=frontend-build /.artifacts/frontend /app/publish/wwwroot

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "QuickBin.API.dll"]
