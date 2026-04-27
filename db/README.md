# db/

DBに直接流すSQLファイル置き場。マイグレーションは `Backend/.../Migrations/` で管理しているので、ここはデータ投入や手動修正用。

## ディレクトリ

- `seed/` … 初期データ投入。番号順に流す前提
- `adhoc/` … 一回限りのデータ修正

## 命名規則

`NNN_短い説明.sql`
例: `001_master_categories.sql`、`002_xxx.sql`

## 実行手順

### 開発環境

```bash
docker exec -i kakeibo-db psql -U postgres -d appdb < db/seed/001_master_categories.sql
```

### 本番

```bash
scp db/seed/001_master_categories.sql raspi5:/tmp/
ssh raspi5 'docker exec -i kakeibo-db psql -U postgres -d appdb < /tmp/001_master_categories.sql'
```

### 全部まとめて流す

```bash
for f in db/seed/*.sql; do
  echo "=== $f ==="
  docker exec -i kakeibo-db psql -U postgres -d appdb < "$f"
done
```

## ルール

- 冪等にする (`WHERE NOT EXISTS` か `ON CONFLICT`)。何度流しても壊れないこと
- `BEGIN; ... COMMIT;` で囲む
- 一度マージしたファイルは編集しない。変更は新しい番号のファイルで追加する
