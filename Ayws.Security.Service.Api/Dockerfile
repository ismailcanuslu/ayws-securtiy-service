FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
USER $APP_UID
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
ARG BUILD_CONFIGURATION=Release
WORKDIR /src
COPY ["Ayws.Security.Service.Api/Ayws.Security.Service.Api.csproj", "Ayws.Security.Service.Api/"]
RUN dotnet restore "Ayws.Security.Service.Api/Ayws.Security.Service.Api.csproj"
COPY . .
WORKDIR "/src/Ayws.Security.Service.Api"
RUN dotnet build "./Ayws.Security.Service.Api.csproj" -c $BUILD_CONFIGURATION -o /app/build

FROM build AS publish
ARG BUILD_CONFIGURATION=Release
RUN dotnet publish "./Ayws.Security.Service.Api.csproj" -c $BUILD_CONFIGURATION -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Ayws.Security.Service.Api.dll"]
