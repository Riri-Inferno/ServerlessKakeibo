/// <reference types="vite/client" />

interface ImportMetaEnv {
  readonly VITE_GOOGLE_CLIENT_ID: string;
  readonly VITE_GITHUB_CLIENT_ID: string;
  readonly VITE_API_BASE_URL: string;
  readonly VITE_ENVIRONMENT: string;
  readonly VITE_APP_VERSION: string;
  readonly VITE_BUILD_DATE: string;
}

interface ImportMeta {
  readonly env: ImportMetaEnv;
}

// window.ENVの型定義（Docker環境用）
interface Window {
  ENV?: {
    API_BASE_URL: string;
    GOOGLE_CLIENT_ID: string;
    GITHUB_CLIENT_ID: string;
    ENVIRONMENT: string;
    APP_VERSION: string;
    BUILD_DATE: string;
  };
}
