# ServerlessKakeibo

Docker環境で実行される家計簿管理アプリケーション

## 概要

ServerlessKakeiboは、レシート自動読み取り機能を備えた家計簿アプリです。

### 主な機能

- レシート画像の自動読み取り（Gemini APIで解析）
- 収支管理
- カテゴリ別集計とグラフ表示
- Google/GitHub OAuth認証
- CSV エクスポート
- ユーザーごとのカテゴリ編集機能

## 技術スタック

- **Frontend**: Vue 3 + TypeScript + Tailwind CSS
- **Backend**: ASP.NET Core 8.0 + Entity Framework Core
- **Database**: PostgreSQL 16
- **Infrastructure**: Docker + Docker Compose
- **Cloud**: Google Cloud Platform (Storage, Vertex AI)

---

## セットアップ

### 前提条件

- Docker Desktop インストール済み
- GCP プロジェクト（レシート読み取り、格納機能を使う場合）
- Google OAuth 2.0 Client ID
- GitHub OAuth App

### 1. 環境変数の設定

#### Backend

```bash
cp Backend/.env.example Backend/.env
```

**Backend/.env** を編集して以下を設定：

```bash
# データベース（Docker環境の場合はそのまま使用可能）
ConnectionStrings__DefaultConnection=Host=db;Port=5432;Database=appdb;Username=postgres;Password=Password

# JWT（32文字以上のランダム文字列を生成）
Authentication__Jwt__SecretKey=<生成したシークレットキー>

# Google OAuth（後述の取得方法を参照）
Authentication__Google__ClientId=<your-client-id>.apps.googleusercontent.com

# GitHub OAuth（後述の取得方法を参照）
Authentication__GitHub__ClientId=<your-client-id>
Authentication__GitHub__ClientSecret=<your-client-secret>

# GCP（レシート読み取り機能を使う場合）
VertexAi__ProjectId=<your-gcp-project-id>
GcpStorage__BucketName=<your-bucket-name>
GcpStorage__ProjectId=<your-gcp-project-id>
```

#### Frontend

```bash
cp Frontend/.env.example Frontend/.env
```

**Frontend/.env** を編集（Docker環境の場合はデフォルト値でOK）：

```bash
VITE_API_BASE_URL=http://localhost:8080
VITE_GOOGLE_CLIENT_ID=<Backend/.envと同じClient ID>
VITE_GITHUB_CLIENT_ID=<Backend/.envと同じClient ID>
```

---

### 2. 認証情報の取得

#### JWT Secret Key の生成

```bash
# Linuxの場合
openssl rand -base64 32

# Node.jsがある場合
node -e "console.log(require('crypto').randomBytes(32).toString('base64'))"
```

#### Google OAuth 2.0

1. [Google Cloud Console](https://console.cloud.google.com/apis/credentials) にアクセス
2. 「認証情報を作成」→「OAuth クライアント ID」
3. アプリケーションの種類：ウェブアプリケーション
4. 承認済みのリダイレクト URI:
   ```
   http://localhost:3000/auth/callback
   ```
5. 作成後、Client ID をコピー

#### GitHub OAuth App

1. [GitHub Developer Settings](https://github.com/settings/developers) にアクセス
2. 「New OAuth App」をクリック
3. 設定:
   - Application name: `ServerlessKakeibo (Dev)`
   - Homepage URL: `http://localhost:3000`
   - Authorization callback URL: `http://localhost:3000/auth/callback`
4. Client ID と Client Secret をコピー

#### GCP サービスアカウント（レシート読み取り、ストレージアップ機能を使う場合）

1. [GCP サービスアカウント](https://console.cloud.google.com/iam-admin/serviceaccounts)で作成
2. 権限: `Storage Admin`, `Vertex AI User`
3. JSON キーをダウンロード
4. 以下に配置:
   ```bash
   cp ~/Downloads/service-account-key.json Backend/ServerlessKakeibo.Api/service_account.json
   ```

**注意**: AI機能（Vertex AI / Google AI Studio）が未設定の場合、レシート読み取り、格納機能は使用できません。手動入力は可能です。

---

### 3. 起動

```bash
docker-compose up -d
```

### 4. アクセス

- **Frontend**: http://localhost:3000
- **Backend API**: http://localhost:8080

---

## デモモードでの確認

外観だけを確認したい場合は、デモモードを使用できます。

```bash
# Frontend/.env
VITE_ENVIRONMENT=demo
```

**デモモードの制限**:
- モックデータが表示されます
- アカウントは架空のもの（バックエンドに実在しない）
- 書き込み操作は行えません
- AI呼び出しなどの外部通信は行われません

**用途**: UI/UXの確認、スクリーンショット撮影など

---

## 停止

```bash
docker-compose down
```

データベースを含めて削除する場合:

```bash
docker-compose down -v
```

---

## 開発

詳細な開発手順は以下を参照してください:

- [セットアップ詳細](./Document/Development/setup.md)
- [モバイル端末でのテスト](./Document/Development/mobile-testing.md)
- [アーキテクチャ](./Document/Architecture/)
- [API仕様](./Document/API/)

---

## ライセンス

Copyright (c) 2026 u.takayo

本ソフトウェアは個人、教育、非商用目的での使用に限定されています。

### 許可されること
- ソースコードの閲覧
- リポジトリのフォーク
- プルリクエストの送信

### 許可されないこと
- 商用利用
- SaaS（サービスとしての提供）
- 変更版・未変更版の再配布

商用利用には作者（u.takayo）からの明示的な許可が必要です。

詳細は [LICENSE](./LICENSE.txt) を参照してください。

---

## 免責事項

本ソフトウェアは「現状のまま」提供され、いかなる保証もありません。

## トラブルシューティング

### データベース接続エラー

```bash
# DBが起動しているか確認
docker-compose ps db

# DBのヘルスチェック
docker-compose exec db pg_isready -U postgres
```

### 環境変数が反映されない

```bash
# コンテナを再ビルド
docker-compose down
docker-compose up --build -d
```

### ログの確認

```bash
# 全体のログ
docker-compose logs

# 特定のサービス
docker-compose logs frontend
docker-compose logs backend
```
