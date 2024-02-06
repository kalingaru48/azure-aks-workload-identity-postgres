FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /src

COPY ["workload-identity/workload-identity.csproj", "workload-identity/"]

RUN dotnet restore "workload-identity/workload-identity.csproj"

COPY . .

WORKDIR "/src/workload-identity"

RUN dotnet build "workload-identity.csproj" -c Release -o /app/build

FROM build AS publish

RUN dotnet publish "workload-identity.csproj" -c Release -o /app/publish

FROM base AS final

WORKDIR /app

COPY --from=publish /app/publish .

RUN ls -la

ENTRYPOINT ["dotnet", "workload-identity.dll"]