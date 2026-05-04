FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["src/InkWell.Communication.Service/InkWell.Communication.Service.csproj", "src/InkWell.Communication.Service/"]
RUN dotnet restore "src/InkWell.Communication.Service/InkWell.Communication.Service.csproj"
COPY . .
WORKDIR "/src/src/InkWell.Communication.Service"
RUN dotnet build "InkWell.Communication.Service.csproj" -c Release -o /app/build
RUN dotnet publish "InkWell.Communication.Service.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "InkWell.Communication.Service.dll"]
