# GCP 認証 + GCS Signed URL 用 IAM 権限セットアップ

このリポジトリは GCP の認証を **ADC (Application Default Credentials)** に統一している。
GCS の署名付きURL は秘密鍵を持たない credential（user / WIF）でも動くよう
**IAM `signBlob` API 経由**で署名する。

そのため認証主体（ローカルなら自分の Google アカウントが impersonate する SA、
本番なら WIF Pod の SA）が、署名用 SA に対して
**`roles/iam.serviceAccountTokenCreator`** を持っている必要がある。

ローカル ADC は SA を impersonate する形で作成し、Vertex AI / GCS / signBlob すべてを
SA の権限で動かす。本番 WIF と挙動が一致し、個人アカウントへの GCP ロール付与も不要。

---

## 環境変数（事前に export しておくと以下のコマンドがコピペで通る）

```bash
export GCP_PROJECT_ID="<your-project-id>"           # 例: project-XXXXXX
export SIGNER_SA="${GCP_PROJECT_ID}@appspot.gserviceaccount.com"
export GCS_BUCKET="<your-bucket-name>"              # 例: serverless-kakeibo
```

> App Engine デフォルト SA を使う場合は `${GCP_PROJECT_ID}@appspot.gserviceaccount.com`。
> 別途 SA を作っているならその email を `SIGNER_SA` に指定する。

---

## 1. IAM Credentials API の有効化（プロジェクトに対して一度だけ）

```bash
gcloud services enable iamcredentials.googleapis.com --project="$GCP_PROJECT_ID"
```

すでに有効なら no-op で終わる。

---

## 2. ローカル開発: 自分のアカウントに `Token Creator` を付与

`gcloud auth application-default login` でログインしている Google アカウントが、
署名用 SA に対して impersonate できるようにする。

```bash
USER_EMAIL="$(gcloud config get-value account)"

gcloud iam service-accounts add-iam-policy-binding "$SIGNER_SA" \
  --member="user:$USER_EMAIL" \
  --role="roles/iam.serviceAccountTokenCreator" \
  --project="$GCP_PROJECT_ID"
```

### 確認

```bash
gcloud iam service-accounts get-iam-policy "$SIGNER_SA" \
  --project="$GCP_PROJECT_ID" \
  --format=json
```

`bindings` 配列に `roles/iam.serviceAccountTokenCreator` のエントリがあり、
`members` に `user:<自分のメール>` が含まれていれば OK。

---

## 3. ローカル ADC を SA インパーソネーションで作成

`--impersonate-service-account` を付けて ADC を作成すると、
ADC が「自分のアカウントが SA を impersonate した状態」になり、
すべての GCP API 呼び出しが SA の権限で動く。

```bash
# 既存 ADC をクリア
gcloud auth application-default revoke

# SA インパーソネーション付きで再ログイン
gcloud auth application-default login \
  --impersonate-service-account="$SIGNER_SA"
```

これで:
- Vertex AI 呼び出し → SA の `Vertex AI User` 権限で実行
- GCS read/write → SA の Storage 権限で実行
- Signed URL 生成 → IAM signBlob で同じ SA が署名

個人アカウントには `iam.serviceAccountTokenCreator`（Step 2）だけあれば良い。

---

## 4. 本番（k3s WIF）: 本番用 SA に同ロールを付与

WIF で kakeibo-backend Pod が impersonate する SA（仮: `kakeibo-backend@${GCP_PROJECT_ID}.iam.gserviceaccount.com`）が、署名用 SA に対して同ロールを持つ必要がある。

```bash
WIF_SA="kakeibo-backend@${GCP_PROJECT_ID}.iam.gserviceaccount.com"

gcloud iam service-accounts add-iam-policy-binding "$SIGNER_SA" \
  --member="serviceAccount:$WIF_SA" \
  --role="roles/iam.serviceAccountTokenCreator" \
  --project="$GCP_PROJECT_ID"
```

> WIF SA を `SIGNER_SA` と同じ SA に統一する場合（self-impersonation）は、
> member 側に同 SA を入れる必要がある。
> 構成は home-raspi-iac の D2（k3s マニフェスト作成）で確定する。

---

## 5. 署名用 SA 自身の権限（参考）

署名用 SA は GCS バケットへの read 権限が必要。Signed URL は SA の権限で動作するため。

```bash
gcloud storage buckets add-iam-policy-binding "gs://${GCS_BUCKET}" \
  --member="serviceAccount:${SIGNER_SA}" \
  --role="roles/storage.objectViewer" \
  --project="$GCP_PROJECT_ID"
```

旧 service_account.json 構成で同 SA を直接使っていた場合は既に付いている可能性が高いが念のため確認。

Vertex AI を呼ぶには `roles/aiplatform.user` も必要:

```bash
gcloud projects add-iam-policy-binding "$GCP_PROJECT_ID" \
  --member="serviceAccount:${SIGNER_SA}" \
  --role="roles/aiplatform.user"
```

---

## トラブルシュート

### `PERMISSION_DENIED: ... does not have iam.serviceAccounts.signBlob`

→ Step 2 / Step 4 の権限付与が反映されていない、または ADC が impersonation 経由になっていない。
反映には最大 1 分程度かかる。`gcloud auth application-default print-access-token` で
取得できるトークンの `aud` を確認するのも有効。

### `API has not been used in project ... or it is disabled`

→ Step 1 の API 有効化が抜けている。`iamcredentials.googleapis.com` を有効化する。

### Signed URL を叩いて `403 SignatureDoesNotMatch`

→ `GcpStorage__SignerServiceAccount` の値が実 SA と異なる、もしくは
SA 自身がバケットへの権限を持っていない（Step 5 を確認）。

### Vertex AI 呼び出しで `403 Permission 'aiplatform.endpoints.predict' denied`

→ ADC が個人アカウント credential のままで impersonation が効いていない。
Step 3 の手順で ADC を作り直す。
