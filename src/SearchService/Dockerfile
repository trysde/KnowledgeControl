FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Shared.Common/Shared.Common.csproj", "src/Shared.Common/"]
COPY ["src/Shared.Contracts/Shared.Contracts.csproj", "src/Shared.Contracts/"]
COPY ["src/SearchService/SearchService.csproj", "src/SearchService/"]
RUN dotnet restore "src/SearchService/SearchService.csproj"

COPY . .
WORKDIR "/src/src/SearchService"
RUN dotnet publish "SearchService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "SearchService.dll"]