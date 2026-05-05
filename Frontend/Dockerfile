# ===================================
# ビルドステージ
# ===================================
FROM node:20-alpine AS build
WORKDIR /app

ARG VITE_API_BASE_URL
ARG VITE_GOOGLE_CLIENT_ID
ARG VITE_GITHUB_CLIENT_ID
ARG VITE_ENVIRONMENT
ARG VITE_APP_VERSION
ARG VITE_BUILD_DATE

ENV VITE_API_BASE_URL=$VITE_API_BASE_URL
ENV VITE_GOOGLE_CLIENT_ID=$VITE_GOOGLE_CLIENT_ID
ENV VITE_GITHUB_CLIENT_ID=$VITE_GITHUB_CLIENT_ID
ENV VITE_ENVIRONMENT=$VITE_ENVIRONMENT
ENV VITE_APP_VERSION=$VITE_APP_VERSION
ENV VITE_BUILD_DATE=$VITE_BUILD_DATE

COPY package*.json ./
RUN npm ci

COPY . .
RUN npm run build

# ===================================
# 実行ステージ
# ===================================
FROM nginx:alpine

COPY --from=build /app/dist /usr/share/nginx/html
COPY nginx.conf /etc/nginx/conf.d/default.conf
COPY entrypoint.sh /entrypoint.sh
RUN chmod +x /entrypoint.sh

EXPOSE 80
ENTRYPOINT ["/entrypoint.sh"]
