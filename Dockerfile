# Est�gio 1: Build
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build
WORKDIR /src

# Copia o arquivo de projeto e restaura as depend�ncias
COPY ["Meetup-WebSocket.csproj", "./"]
RUN dotnet restore "Meetup-WebSocket.csproj"

# Copia todo o c�digo fonte e compila
COPY . .
RUN dotnet build "Meetup-WebSocket.csproj" -c Release -o /app/build

# Est�gio 2: Publish
FROM build AS publish
RUN dotnet publish "Meetup-WebSocket.csproj" -c Release -o /app/publish /p:UseAppHost=false

# Est�gio 3: Runtime
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS final
WORKDIR /app
EXPOSE 8080
EXPOSE 8081

# Copia os arquivos publicados
COPY --from=publish /app/publish .

# Define vari�veis de ambiente
ENV ASPNETCORE_URLS=http://+:8080
ENV ASPNETCORE_ENVIRONMENT=Production

ENTRYPOINT ["dotnet", "Meetup-WebSocket.dll"]