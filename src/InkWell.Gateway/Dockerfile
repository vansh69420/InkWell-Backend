FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/InkWell.Gateway/InkWell.Gateway.csproj", "src/InkWell.Gateway/"]
RUN dotnet restore "src/InkWell.Gateway/InkWell.Gateway.csproj"
COPY . .
WORKDIR "/src/src/InkWell.Gateway"
RUN dotnet build "InkWell.Gateway.csproj" -c Release -o /app/build
RUN dotnet publish "InkWell.Gateway.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
COPY src/InkWell.Gateway/ocelot.json .
COPY src/InkWell.Gateway/ocelot.Production.json .
ENTRYPOINT ["dotnet", "InkWell.Gateway.dll"]
