# ビルドステージ
FROM node:20-alpine AS build
WORKDIR /app

# ビルド引数を定義
ARG VITE_API_BASE_URL
ARG VITE_GOOGLE_CLIENT_ID
ARG VITE_GITHUB_CLIENT_ID
ARG VITE_APP_NAME=ServerlessKakeibo
ARG VITE_ENABLE_DEBUG=false

# 環境変数として設定
ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_GOOGLE_CLIENT_ID=$VITE_GOOGLE_CLIENT_ID
ENV VITE_GITHUB_CLIENT_ID=$VITE_GITHUB_CLIENT_ID
ENV VITE_APP_NAME=$VITE_APP_NAME
ENV VITE_ENABLE_DEBUG=$VITE_ENABLE_DEBUG

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

# 実行ステージ
FROM nginx:alpine

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf

EXPOSE 80

CMD ["nginx", "-g", "daemon off;"]
