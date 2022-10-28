FROM mcr.microsoft.com/dotnet/runtime:6.0 AS base
WORKDIR /app

FROM mcr.microsoft.com/dotnet/sdk:6.0 AS build
WORKDIR /src
COPY ["ChemDoserProxy/ChemDoserProxy.csproj", "ChemDoserProxy/"]
RUN dotnet restore "ChemDoserProxy/ChemDoserProxy.csproj"
COPY . .
WORKDIR "/src/ChemDoserProxy"
RUN dotnet build "ChemDoserProxy.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "ChemDoserProxy.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "ChemDoserProxy.dll"]
