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

# ビルド引数として受け取る
ARG VITE_API_BASE_URL=http://localhost:8080
ARG VITE_GOOGLE_CLIENT_ID=""
ARG VITE_GITHUB_CLIENT_ID=""
ARG VITE_ENVIRONMENT=development
ARG VITE_APP_VERSION=1.0.0
ARG VITE_BUILD_DATE=2025-01-01

# ARG から ENV に設定（デフォルト値をリポジトリ変数で置き換え）
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_GOOGLE_CLIENT_ID=$VITE_GOOGLE_CLIENT_ID
ENV VITE_GITHUB_CLIENT_ID=$VITE_GITHUB_CLIENT_ID
ENV VITE_ENVIRONMENT=$VITE_ENVIRONMENT
ENV VITE_APP_VERSION=$VITE_APP_VERSION
ENV VITE_BUILD_DATE=$VITE_BUILD_DATE

COPY --from=build /app/dist /usr/share/nginx/html

# nginx設定ファイル
COPY nginx.conf /etc/nginx/conf.d/default.conf

# entrypointスクリプトをコピー
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

EXPOSE 80

# entrypoint.shで環境変数を処理してnginx起動
ENTRYPOINT ["/entrypoint.sh"]
