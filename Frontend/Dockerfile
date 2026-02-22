# ===================================
# ビルドステージ
# ===================================
FROM node:20-alpine AS build
WORKDIR /app

# 依存関係をインストール
COPY package*.json ./
RUN npm ci

# ソースコードをコピーしてビルド
COPY . .
RUN npm run build

# ===================================
# 実行ステージ
# ===================================
FROM nginx:alpine

# ビルド成果物をコピー
COPY --from=build /app/dist /usr/share/nginx/html

# nginx設定ファイル
COPY nginx.conf /etc/nginx/conf.d/default.conf

# entrypointスクリプトをコピー
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

# デフォルト値（オプション：docker-compose.ymlで上書き可能）
ENV VITE_API_BASE_URL=http://localhost:8080
ENV VITE_GOOGLE_CLIENT_ID=""
ENV VITE_GITHUB_CLIENT_ID=""
ENV VITE_ENVIRONMENT=development
ENV VITE_APP_VERSION=1.0.0
ENV VITE_BUILD_DATE=2025-01-01

EXPOSE 80

# entrypoint.shで環境変数を処理してnginx起動
ENTRYPOINT ["/entrypoint.sh"]
