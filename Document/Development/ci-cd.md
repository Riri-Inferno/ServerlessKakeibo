# CI/CD 運用ガイド

このリポジトリの GitHub Actions ワークフローと運用フロー。
image registry は **GHCR**（GitHub Container Registry）を本番採用、GAR は disabled の手動フォールバックとして残置。

---

## ワークフロー一覧

| File | Trigger | 用途 | 出力 |
|---|---|---|---|
| [`ci.yml`](../../.github/workflows/ci.yml) | PR (main / develop) | backend / frontend の docker build 確認のみ | なし（push しない） |
| [`build-dev.yml`](../../.github/workflows/build-dev.yml) | `push: branches: [develop]` | dev image の継続ビルド | GHCR `:dev`, `:dev-<sha>`（arm64） |
| [`release.yml`](../../.github/workflows/release.yml) | `release: published` / `workflow_dispatch` | 本番 image | GHCR `:latest`, `:<version>`（arm64） |
| [`deploy-demo.yml`](../../.github/workflows/deploy-demo.yml) | main push（Frontend/**） | GitHub Pages のデモ更新 | Pages |
| [`push-to-gar.yml`](../../.github/workflows/push-to-gar.yml) | `workflow_dispatch` のみ | **disable 運用**。GAR への push が再度必要になった場合のフォールバック | GAR `:latest`, `:<sha>`（arm64） |

image build は **`linux/arm64` のみ**。raspi5 (arm64) でしか動かさない前提。

## ランナー方針

- 全ての image build ジョブは **`runs-on: ubuntu-22.04-arm`**（GitHub のネイティブ arm64 ランナー、public repo は無料）
- QEMU クロスビルドは使わない。理由は Node 20 を amd64 ホスト上で QEMU arm64 エミュレーションすると `qemu: uncaught target signal 4 (Illegal instruction)` で `npm ci` が落ちるため
- ローカル開発は amd64 マシンが多いため `docker compose up --build` で各自ビルドする想定。GHCR から amd64 image を pull するシナリオは現状ない

### amd64 image が後から必要になったら

別ワークフロー（例: `build-dev-amd64.yml`）として追加する。タグは `:dev-amd64` 等で名前空間を分離するのが簡単。multi-arch manifest にまとめたい場合は各ワークフローで digest を export し、後段の merge ジョブで `docker buildx imagetools create` する公式パターンを使う。

---

## 通常運用フロー

### 開発（develop ブランチ）

1. feature ブランチで作業 → develop に PR
2. PR 上で `ci.yml` が build 成功することを確認
3. develop にマージ
4. `build-dev.yml` が自動発火 → GHCR に `:dev` / `:dev-<sha>` が更新される
5. 開発用環境（あれば）が `:dev` を pull して反映

### 本番リリース

1. develop → main に PR を上げてマージ
2. GitHub UI から **Release** を作成（タグ例: `v1.0.0`）
3. `release.yml` が自動発火 → GHCR に `:latest` と `:v1.0.0` が更新される
4. k3s 側（home-raspi-iac）が新タグを pull して反映

### 緊急時の手動 image push

`release.yml` の `workflow_dispatch` トリガーから:
- Actions タブ → `Release & Push to GHCR` → `Run workflow`
- `version` 入力欄に任意のタグ（例: `hotfix-20260601`）
- 未入力なら commit sha 先頭 7 文字がタグになる

### GAR への push が必要になった場合

`Push to GAR (manual)` ワークフローを GitHub UI で **enable** に戻し、`Run workflow` で発火。
通常は disable のままにしておく。

---

## マージ後・初回セットアップ

GHCR ベースの構成に切り替えた直後、または fork 後に必要な作業。

### 1. GHCR package を public 化

初回 push で `ghcr.io/<owner>/<repo>/{backend,frontend}` の package が作られる。
default は private なので、k3s が `imagePullSecret` 無しで pull できるよう public 化する。

1. GitHub の Profile（Organization なら Org トップ） → **Packages** タブ
2. `backend` を選択 → 右上の **Package settings**
3. `Danger Zone` の **Change visibility** → `Public`
4. `frontend` も同様

### 2. `Push to GAR (manual)` を disable

通常運用では使わないため:

1. Actions タブ → 左サイドバーの `Push to GAR (manual)` を選択
2. 右上の `...` → **Disable workflow**

### 3. 不要になった Secrets / Vars の整理（任意）

以下はもう参照されていないため、GitHub の Settings → Secrets/Variables から削除可能。

- `secrets.SERVICE_ACCOUNT_JSON`（service_account.json 焼き込み廃止）
- `secrets.RASPI_SSH_PRIVATE_KEY`、`secrets.RASPI_SSH_USER`（SSH デプロイ廃止）

下記は `push-to-gar.yml` を再有効化する可能性があるなら残す:

- `secrets.WIF_PROVIDER`、`secrets.WIF_SERVICE_ACCOUNT`
- `vars.GCP_PROJECT_ID`、`vars.GAR_LOCATION`、`vars.GAR_REPOSITORY`

### 4. arm64 image 確認

raspi5 等の arm64 環境から:

```bash
docker manifest inspect ghcr.io/<owner>/<repo>/backend:dev | grep -E 'architecture|os'
docker manifest inspect ghcr.io/<owner>/<repo>/frontend:dev | grep -E 'architecture|os'
```

`linux/arm64` が含まれていれば OK。amd64 マシンから pull しようとすると `no matching manifest` で失敗するのが正常。

---

## トラブルシュート

### `denied: installation not allowed to Create organization package`

GHCR への初回 push 時に出る場合、リポジトリの Settings → Actions → General で
**Workflow permissions** を `Read and write permissions` に変更する必要がある。

### k3s からの `ImagePullBackOff` で `manifest unknown` / `no matching manifest`

`linux/arm64` の manifest が含まれていない、または image が public 化されていない。
`build-dev.yml` / `release.yml` の `platforms: linux/arm64` 設定 と GHCR package の
visibility（public）を確認。

### CI が `qemu: uncaught target signal 4 (Illegal instruction)` で落ちる

QEMU 経由のクロスビルドが復活している兆候。`runs-on: ubuntu-22.04-arm`（ネイティブ arm64）
で動かしているか、`setup-qemu-action` が誤って復活していないかを確認。

### `release.yml` を手動実行したのにタグが `manual` 等になった

`workflow_dispatch` の `version` 入力欄を空のまま実行した場合、commit sha 先頭 7 文字が
タグになる。明示的にタグ名を入れるか、空欄なら sha タグで運用する。

### `Push to GAR (manual)` を実行したら `WIF_PROVIDER not set` と出る

GitHub の Secrets に `WIF_PROVIDER` / `WIF_SERVICE_ACCOUNT` が登録されていない、
または disable 後に削除済の状態。「マージ後・初回セットアップ」の Secrets 整理を確認。
