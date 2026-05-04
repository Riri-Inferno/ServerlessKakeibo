# k3s 環境変数仕分け

raspi5 上の k3s に backend / frontend / db を乗せる際の、各環境変数の Kubernetes リソース割当。
具体的なマニフェスト（Deployment / Service / ConfigMap / SealedSecret 等）は
[home-raspi-iac](https://github.com/Riri-Inferno/home-raspi-iac) リポジトリで管理する。
本ドキュメントは「kakeibo 側として何を、どこに置くべきか」の仕分け表。


---

## Backend

### ConfigMap（非機密）

| Key | 値の例 | 備考 |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Production` | |
| `ASPNETCORE_URLS` | `http://+:8080` | コンテナ内リッスン |
| `GOOGLE_APPLICATION_CREDENTIALS` | `/etc/gcp/wif.json` | WIF JSON のマウント先パス |
| `GcpAuth__DefaultScopes__0` | `https://www.googleapis.com/auth/cloud-platform` | |
| `GcpAuth__UseCache` | `True` | |
| `GcpAuth__CacheDurationMinutes` | `60` | |
| `VertexAi__ProjectId` | `<gcp-project-id>` | |
| `GcpStorage__BucketName` | `<bucket-name>` | |
| `GcpStorage__ProjectId` | `<gcp-project-id>` | |
| `GcpStorage__IsPublicBucket` | `false` | |
| `GcpStorage__GenerateSignedUrl` | `true` | |
| `GcpStorage__SignedUrlExpirationHours` | `1` | |
| `GcpStorage__SignerServiceAccount` | `<sa-email>` | SA email は識別子。詳細は [iam-signer-setup.md](./iam-signer-setup.md) |
| `Authentication__Jwt__Issuer` | `ServerlessKakeiboApi` | |
| `Authentication__Jwt__Audience` | `ServerlessKakeiboApp` | |
| `Authentication__Jwt__ExpirationMinutes` | `15` | |
| `Authentication__Jwt__RefreshTokenExpirationDays` | `7` | |
| `Authentication__Google__ClientId` | `<id>.apps.googleusercontent.com` | OAuth Client ID は公開前提 |
| `Authentication__GitHub__ClientId` | `<github-client-id>` | 同上 |
| `AllowedOrigins__0` | `https://kakeibo.riri-inferno.com` | CORS 許可元 |
| `DefaultTenantId` | `deadeade-0001-0000-0000-000000000001` | |
| `POSTGRES_USER` | `postgres` | DB ユーザー名は慣例的に非機密 |
| `POSTGRES_DB` | `appdb` | |

### SealedSecret（機密）

| Key | 用途 |
|---|---|
| `ConnectionStrings__DefaultConnection` | DB 接続文字列（password を含むため丸ごと Secret 化） |
| `Authentication__Jwt__SecretKey` | JWT 署名鍵 |
| `Authentication__GitHub__ClientSecret` | GitHub OAuth Client Secret |
| `GoogleAiStudio__ApiKey` | （使用する場合のみ）Vertex AI 代替の API Key |
| `POSTGRES_PASSWORD` | DB パスワード |

### ファイルマウント

| Path | 内容 | リソース | 備考 |
|---|---|---|---|
| `/etc/gcp/wif.json` | WIF 設定 JSON | ConfigMap | WIF JSON 自体は識別子の集合で機密ではない。Pod に projected SA token を併せてマウントすることで認証が成立する |
| `/var/run/secrets/tokens/gcp-token` | Pod の SA token（projected volume） | k8s 側で自動 | Pod spec の `volumes.projected.sources.serviceAccountToken` で指定（home-raspi-iac 側） |

---

## Frontend

現状は build-time に Vite ARG で焼き込み（CI workflows の `build-args`）。
runtime injection 経路（`entrypoint.sh` の `config.js` 生成）も並存している。
build-time 焼き込みのまま k3s に上げる場合、frontend 側に必要な ConfigMap / Secret はなし。

| Key | 現状の注入方法 | 備考 |
|---|---|---|
| `VITE_API_BASE_URL` | build-time | image に固定 |
| `VITE_GOOGLE_CLIENT_ID` | build-time | 同上 |
| `VITE_GITHUB_CLIENT_ID` | build-time | 同上 |
| `VITE_ENVIRONMENT` | build-time | 同上（`production` 固定） |

runtime injection に一本化する方針へ倒した場合、これらは ConfigMap 行きになる。

---

## DB (PostgreSQL)

| 項目 | 場所 |
|---|---|
| `POSTGRES_USER`, `POSTGRES_DB` | backend と同じ ConfigMap を参照 |
| `POSTGRES_PASSWORD` | backend と同じ Secret を参照 |
| データ永続化 | PVC（StatefulSet または Deployment + PVC、home-raspi-iac で決定） |

---

## k3s クラスタ側の責務（kakeibo repo の責務外）

以下は home-raspi-iac リポジトリで管理する想定。kakeibo repo には設定値を持たない。

| 項目 | 概要 |
|---|---|
| Cloudflare Tunnel | `kakeibo.riri-inferno.com` / `api-kakeibo.riri-inferno.com` の外部公開。`CLOUDFLARE_TUNNEL_TOKEN` も home-raspi-iac 側 |
| WIF 設定 | Workload Identity Pool / Provider の作成、k8s SA への impersonation 紐付け |
| SealedSecret コントローラ | クラスタにインストール + 公開鍵で encrypt したマニフェストを管理 |
| Image Pull | GHCR の image を public にしているため imagePullSecret 不要 |

---

## 関連ドキュメント

- [iam-signer-setup.md](./iam-signer-setup.md) — GCP IAM 権限と署名用 SA のセットアップ
- [ci-cd.md](./ci-cd.md) — image push と運用フロー
