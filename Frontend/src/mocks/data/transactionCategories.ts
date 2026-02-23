/**
* デモ用の取引カテゴリマスターデータ
* isHidden: false = 表示中、true = 削除済み
*/
export const mockTransactionCategories = [
  // ===== システムデフォルト（支出） =====
  {
    id: "11111111-1111-1111-1111-000000000001",
    name: "食費",
    colorCode: "#FF5733",
    isIncome: false,
    isCustom: false,
    isHidden: false,
  },
  {
    id: "11111111-1111-1111-1111-000000000002",
    name: "外食",
    colorCode: "#FF8C33",
    isIncome: false,
    isCustom: false,
    isHidden: false,
  },
  {
    id: "11111111-1111-1111-1111-000000000003",
    name: "日用品",
    colorCode: "#4CAF50",
    isIncome: false,
    isCustom: false,
    isHidden: false,
  },
  {
    id: "11111111-1111-1111-1111-000000000004",
    name: "交通費",
    colorCode: "#2196F3",
    isIncome: false,
    isCustom: false,
    isHidden: false,
  },
  {
    id: "11111111-1111-1111-1111-000000000005",
    name: "医療・健康",
    colorCode: "#9C27B0",
    isIncome: false,
    isCustom: false,
    isHidden: false,
  },

  // ===== システムデフォルト（収入） =====
  {
    id: "11111111-1111-1111-1111-000000000100",
    name: "給与",
    colorCode: "#4CAF50",
    isIncome: true,
    isCustom: false,
    isHidden: false,
  },
  {
    id: "11111111-1111-1111-1111-000000000101",
    name: "その他収入",
    colorCode: "#8BC34A",
    isIncome: true,
    isCustom: false,
    isHidden: false,
  },

  // ===== カスタムカテゴリ（支出） =====
  {
    id: "37f02cfd-cc4f-4618-98df-6bafe9037a1d",
    name: "娯楽",
    colorCode: "#FFC107",
    isIncome: false,
    isCustom: true,
    isHidden: false,
  },
  {
    id: "728bab2d-01f2-4b6a-9e86-614c2629e64c",
    name: "書籍・雑誌",
    colorCode: "#795548",
    isIncome: false,
    isCustom: true,
    isHidden: false,
  },
  {
    id: "c1ffc792-f984-44fb-85f5-281b1db54b1f",
    name: "食材費",
    colorCode: "#66BB6A",
    isIncome: false,
    isCustom: true,
    isHidden: false,
  },
  {
    id: "18cf6a37-a1b4-4e83-a19d-19ee50821a55",
    name: "カフェ・喫茶",
    colorCode: "#8D6E63",
    isIncome: false,
    isCustom: true,
    isHidden: false,
  },

  // ===== カスタムカテゴリ（収入） =====
  {
    id: "754d591e-17fd-43f5-9806-ae20b61e9539",
    name: "賞与",
    colorCode: "#66BB6A",
    isIncome: true,
    isCustom: true,
    isHidden: false,
  },

  // ===== 削除済みカテゴリ（デモ用） =====
  {
    id: "deleted-category-001",
    name: "旧・交際費",
    colorCode: "#9E9E9E",
    isIncome: false,
    isCustom: true,
    isHidden: true,
  },
  {
    id: "deleted-category-002",
    name: "旧・副業収入",
    colorCode: "#BDBDBD",
    isIncome: true,
    isCustom: true,
    isHidden: true,
  },
];
