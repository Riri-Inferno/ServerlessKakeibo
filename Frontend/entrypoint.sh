#!/bin/sh

# 環境変数から config.js を生成
cat > /usr/share/nginx/html/config.js << EOF
window.ENV = {
  API_BASE_URL: "${VITE_API_BASE_URL}",
  GOOGLE_CLIENT_ID: "${VITE_GOOGLE_CLIENT_ID}",
  GITHUB_CLIENT_ID: "${VITE_GITHUB_CLIENT_ID}",
  ENVIRONMENT: "${VITE_ENVIRONMENT}",
  APP_VERSION: "${VITE_APP_VERSION}",
  BUILD_DATE: "${VITE_BUILD_DATE}"
};
EOF

echo "=========================================="
echo "Frontend Configuration Generated"
echo "=========================================="
cat /usr/share/nginx/html/config.js
echo "=========================================="

# nginx起動
exec nginx -g 'daemon off;'
