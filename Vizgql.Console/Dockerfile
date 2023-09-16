FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["Vizgql.Console/Vizgql.Console.csproj", "Vizgql.Console/"]
RUN dotnet restore "Vizgql.Console/Vizgql.Console.csproj"
COPY . .
WORKDIR "/src/Vizgql.Console"
RUN dotnet build "Vizgql.Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "Vizgql.Console.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "Vizgql.Console.dll"]
