# マイグレ
## プロジェクトディレクトリへ移動
cd Backend/ServerlessKakeibo.Api

## マイグレーション作成
dotnet ef migrations add InitialCreate \
  --output-dir Infrastructure/Migrations

## 4データベースへ適用
dotnet ef database update
