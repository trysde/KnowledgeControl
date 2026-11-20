FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY ["src/Shared.Common/Shared.Common.csproj", "src/Shared.Common/"]
COPY ["src/Shared.Contracts/Shared.Contracts.csproj", "src/Shared.Contracts/"]
COPY ["src/TestTakingService/TestTakingService.csproj", "src/TestTakingService/"]
RUN dotnet restore "src/TestTakingService/TestTakingService.csproj"

COPY . .
WORKDIR "/src/src/TestTakingService"
RUN dotnet publish "TestTakingService.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app
EXPOSE 8080
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "TestTakingService.dll"]