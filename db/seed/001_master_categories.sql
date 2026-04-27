-- ============================================================
-- 001_master_categories.sql
-- マスタカテゴリ初期データ
--
-- 対象:
--   public."TransactionCategoryMasters"  (取引カテゴリマスタ)
--   public."ItemCategoryMasters"         (商品カテゴリマスタ: 支出用)
--   public."IncomeItemCategoryMasters"   (給与項目カテゴリマスタ: 収入用)
--
-- 備考:
--   - RowVersion(xmin) はPostgreSQL自動採番のためカラム指定なし
--   - TenantId / CreatedBy / UpdatedBy はシステムテナントID固定
--   - Code 重複時はスキップ(冪等)
-- ============================================================

BEGIN;

\set system_id '\'deadeade-0001-0000-0000-000000000001\''


-- ------------------------------------------------------------
-- TransactionCategoryMasters (取引カテゴリ: 支出5 + 収入5)
-- ------------------------------------------------------------
INSERT INTO public."TransactionCategoryMasters" (
    "Id",
    "Name", "Code", "ColorCode", "DisplayOrder",
    "IsIncome", "IsSystemDefault",
    "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy",
    "IsDeleted", "TenantId"
)
SELECT v."Id",
       v."Name", v."Code", v."ColorCode", v."DisplayOrder",
       v."IsIncome", true,
       NOW(), :system_id::uuid, NOW(), NULL,
       false, :system_id::uuid
FROM (VALUES
    -- 支出
    (gen_random_uuid(), '食費',       'Food',           '#F87171',  1, false),
    (gen_random_uuid(), '交通費',     'Transport',      '#60A5FA',  2, false),
    (gen_random_uuid(), '日用品',     'DailyGoods',     '#34D399',  3, false),
    (gen_random_uuid(), '光熱費',     'Utility',        '#FBBF24',  4, false),
    (gen_random_uuid(), '娯楽',       'Leisure',        '#A78BFA',  5, false),
    -- 収入
    (gen_random_uuid(), '給与',       'Salary',         '#10B981', 11, true),
    (gen_random_uuid(), '賞与',       'Bonus',          '#059669', 12, true),
    (gen_random_uuid(), '副業',       'SideJob',        '#84CC16', 13, true),
    (gen_random_uuid(), '投資収益',   'InvestmentGain', '#22D3EE', 14, true),
    (gen_random_uuid(), 'その他収入', 'OtherIncome',    '#9CA3AF', 15, true)
) AS v("Id", "Name", "Code", "ColorCode", "DisplayOrder", "IsIncome")
WHERE NOT EXISTS (
    SELECT 1 FROM public."TransactionCategoryMasters" m
    WHERE m."Code" = v."Code"
);


-- ------------------------------------------------------------
-- ItemCategoryMasters (商品カテゴリマスタ: 支出用)
-- ------------------------------------------------------------
INSERT INTO public."ItemCategoryMasters" (
    "Id",
    "Name", "Code", "ColorCode", "DisplayOrder",
    "IsSystemDefault",
    "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy",
    "IsDeleted", "TenantId"
)
SELECT v."Id",
       v."Name", v."Code", v."ColorCode", v."DisplayOrder",
       true,
       NOW(), :system_id::uuid, NOW(), NULL,
       false, :system_id::uuid
FROM (VALUES
    (gen_random_uuid(), '食料品', 'Grocery',  '#F87171', 1),
    (gen_random_uuid(), '外食',   'Dining',   '#FB923C', 2),
    (gen_random_uuid(), '衣類',   'Clothing', '#A78BFA', 3),
    (gen_random_uuid(), '書籍',   'Book',     '#60A5FA', 4),
    (gen_random_uuid(), '医療費', 'Medical',  '#F472B6', 5)
) AS v("Id", "Name", "Code", "ColorCode", "DisplayOrder")
WHERE NOT EXISTS (
    SELECT 1 FROM public."ItemCategoryMasters" m
    WHERE m."Code" = v."Code"
);


-- ------------------------------------------------------------
-- IncomeItemCategoryMasters (給与項目カテゴリマスタ: 収入用)
-- ------------------------------------------------------------
INSERT INTO public."IncomeItemCategoryMasters" (
    "Id",
    "Name", "Code", "ColorCode", "DisplayOrder",
    "IsSystemDefault",
    "CreatedAt", "CreatedBy", "UpdatedAt", "UpdatedBy",
    "IsDeleted", "TenantId"
)
SELECT v."Id",
       v."Name", v."Code", v."ColorCode", v."DisplayOrder",
       true,
       NOW(), :system_id::uuid, NOW(), NULL,
       false, :system_id::uuid
FROM (VALUES
    (gen_random_uuid(), '基本給',     'BaseSalary',       '#10B981', 1),
    (gen_random_uuid(), '残業手当',   'OvertimePay',      '#34D399', 2),
    (gen_random_uuid(), '通勤手当',   'CommuteAllowance', '#22D3EE', 3),
    (gen_random_uuid(), '住宅手当',   'HousingAllowance', '#84CC16', 4),
    (gen_random_uuid(), '賞与',       'Bonus',            '#059669', 5)
) AS v("Id", "Name", "Code", "ColorCode", "DisplayOrder")
WHERE NOT EXISTS (
    SELECT 1 FROM public."IncomeItemCategoryMasters" m
    WHERE m."Code" = v."Code"
);


COMMIT;
