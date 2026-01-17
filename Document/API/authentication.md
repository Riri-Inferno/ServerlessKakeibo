# 認証API仕様

## 概要

本APIは Google OAuth 2.0 を使用した認証システムを提供します。

### 認証方式
- **JWT（JSON Web Token）** ベースの認証
- **AccessToken**: 短期間有効（15分）、API呼び出しに使用
- **RefreshToken**: 長期間有効（7日）、AccessToken の更新に使用

### セキュリティ特性
- ✅ One-Time Use RefreshToken（1回使用したら無効化）
- ✅ トランザクション保護による競合防止
- ✅ トークンローテーション（Token Rotation）

---

## エンドポイント

### 1. Google ログイン

Google OAuth 2.0 で取得した ID Token を検証し、JWT トークンを発行します。

#### リクエスト

```http
POST /api/auth/google
Content-Type: application/json

{
  "idToken": "eyJhbGciOiJSUzI1NiIsImtpZCI6..."
}
```

#### レスポンス（成功）

```json
{
  "status": "Success",
  "message": null,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6...",
    "refreshToken": "hN046x1/CDGVAnjI/PX92H4V...",
    "userId": "aaa88a5c-113d-4740-9e59-1a9789fdf146",
    "displayName": "りり",
    "pictureUrl": "https://lh3.googleusercontent.com/..."
  }
}
```

#### エラーレスポンス

```json
{
  "status": "Unauthorized",
  "message": "認証に失敗しました。",
  "data": null
}
```

#### 注意事項
- **Google ID Token** を使用してください（Access Token ではありません）
- ID Token は `eyJ` で始まる長い文字列です
- 初回ログイン時は自動的にユーザーが作成されます

---

### 2. トークン更新

RefreshToken を使用して新しい AccessToken を取得します。

#### リクエスト

```http
POST /api/auth/refresh
Content-Type: application/json

{
  "refreshToken": "hN046x1/CDGVAnjI/PX92H4V..."
}
```

#### レスポンス（成功）

```json
{
  "status": "Success",
  "message": null,
  "data": {
    "accessToken": "eyJhbGciOiJIUzI1NiIsInR5cCI6...",
    "refreshToken": "Ku0YXWTWicchGMzTDnVRuJU...",
    "userId": "aaa88a5c-113d-4740-9e59-1a9789fdf146",
    "displayName": "りり",
    "pictureUrl": "https://lh3.googleusercontent.com/..."
  }
}
```

#### エラーレスポンス（トークン無効）

```json
{
  "status": "Unauthorized",
  "message": "リフレッシュトークンが無効または期限切れです。再度ログインしてください。",
  "data": null
}
```

#### 重要な仕様
- ✅ **One-Time Use**: 1回使用したら古い RefreshToken は無効化されます
- ✅ 新しい AccessToken と RefreshToken の両方が返されます
- ⚠️ 古い RefreshToken を再利用すると 401 エラーになります

---

### 3. 現在のユーザー情報取得

JWT トークンから現在ログイン中のユーザー情報を取得します。

#### リクエスト

```http
GET /api/auth/me
Authorization: Bearer eyJhbGciOiJIUzI1NiIsInR5cCI6...
```

#### レスポンス（成功）

```json
{
  "status": "Success",
  "message": null,
  "data": {
    "userId": "aaa88a5c-113d-4740-9e59-1a9789fdf146",
    "displayName": "りり",
    "email": "riri-inferno@gmail.com"
  }
}
```

#### エラーレスポンス（認証失敗）

```json
{
  "status": "Unauthorized",
  "message": "認証情報が不正です。",
  "data": null
}
```

---

## JWT トークンの構造

### AccessToken の Claims

```json
{
  "sub": "ユーザーID",
  "email": "メールアドレス",
  "name": "表示名",
  "tenant_id": "テナントID",
  "jti": "JWT ID（一意な識別子）",
  "exp": "有効期限（Unix timestamp）",
  "iss": "発行者（ServerlessKakeiboApi）",
  "aud": "対象者（ServerlessKakeiboApp）"
}
```

### RefreshToken の構造

- ランダムな 64 バイトの Base64 文字列
- 暗号論的に安全な乱数生成器（`RandomNumberGenerator`）を使用

---


## セキュリティ上の注意事項

### ✅ 実装済みのセキュリティ対策

1. **One-Time Use RefreshToken**
   - 1回使用したら無効化されるため、盗まれたトークンの悪用を防止

2. **トランザクション保護**
   - 同時リクエストでも1つしか成功しない（競合防止）

3. **短命な AccessToken**
   - 15分で期限切れになるため、盗まれた場合の被害を最小化

4. **HTTPS必須（本番環境）**
   - トークンの盗聴を防止するため、本番環境では必ずHTTPSを使用してください

### ⚠️ フロント実装時の注意点

1. **トークンの有効期限**
   - AccessToken: 15分
   - RefreshToken: 7日
   - 7日後は再ログインが必要です

2. **エラーハンドリング**
   - 401エラー時は自動リフレッシュを試みる
   - リフレッシュ失敗時はログアウトしてログイン画面に遷移

---

## トラブルシューティング

### 問題: 401 Unauthorized が連続して発生する

**原因**: AccessToken の有効期限切れ + RefreshToken の自動更新が機能していない

**解決方法**:
1. Axios Interceptor が正しく設定されているか確認
2. `_retry` フラグで無限ループを防いでいるか確認
3. ブラウザの開発者ツールで RefreshToken が保存されているか確認

### 問題: RefreshToken が「無効」と言われる

**原因**: 
- トークンが既に使用済み（One-Time Use）
- 有効期限（7日）が切れている

**解決方法**:
- 再度 Google ログインを実行してください

### 問題: Google ID Token の検証に失敗する

**原因**: 
- Google Access Token を送信している（ID Token ではない）
- ClientID が appsettings.json と一致していない

**解決方法**:
1. Google OAuth Playground で正しい **ID Token** を取得
2. appsettings.json の `Authentication:Google:ClientId` を確認

---

## 関連ドキュメント

- [認証フロー図](../Architecture/auth-flow.md)
