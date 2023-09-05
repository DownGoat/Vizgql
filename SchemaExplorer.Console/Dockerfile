FROM mcr.microsoft.com/dotnet/runtime:7.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:7.0 AS build
WORKDIR /src
COPY ["SchemaExplorer.Console/SchemaExplorer.Console.csproj", "SchemaExplorer.Console/"]
RUN dotnet restore "SchemaExplorer.Console/SchemaExplorer.Console.csproj"
COPY . .
WORKDIR "/src/SchemaExplorer.Console"
RUN dotnet build "SchemaExplorer.Console.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "SchemaExplorer.Console.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "SchemaExplorer.Console.dll"]
