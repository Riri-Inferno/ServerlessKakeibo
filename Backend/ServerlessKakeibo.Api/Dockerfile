# ===================================
# ビルドステージ
# ===================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src

COPY ["ServerlessKakeibo.Api/ServerlessKakeibo.Api.csproj", "ServerlessKakeibo.Api/"]
RUN dotnet restore "ServerlessKakeibo.Api/ServerlessKakeibo.Api.csproj"

COPY . .
WORKDIR "/src/ServerlessKakeibo.Api"
RUN dotnet build "ServerlessKakeibo.Api.csproj" -c Release -o /app/build

# ===================================
# 実行ステージ
# ===================================
FROM build AS publish
RUN dotnet publish "ServerlessKakeibo.Api.csproj" -c Release -o /app/publish /p:UseAppHost=false

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .

# service_account.json をコピー
COPY service_account.json /app/service_account.json
RUN chown app:app /app/service_account.json && chmod 600 /app/service_account.json

ENTRYPOINT ["dotnet", "ServerlessKakeibo.Api.dll"]
