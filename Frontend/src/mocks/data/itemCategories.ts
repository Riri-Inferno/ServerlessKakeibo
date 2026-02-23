/**
 * デモ用の商品カテゴリマスターデータ（支出用・明細レベル）
 * isHidden: false = 表示中、true = 削除済み
 */
export const mockItemCategories = [
  // ===== システムデフォルト =====
  {
    id: "22222222-2222-2222-2222-000000000001",
    name: "食品",
    colorCode: "#FF5733",
    isCustom: false,
    isHidden: false,
  },
  {
    id: "22222222-2222-2222-2222-000000000002",
    name: "飲料",
    colorCode: "#2196F3",
    isCustom: false,
    isHidden: false,
  },
  {
    id: "22222222-2222-2222-2222-000000000003",
    name: "お菓子・スナック",
    colorCode: "#FFC107",
    isCustom: false,
    isHidden: false,
  },
  {
    id: "22222222-2222-2222-2222-000000000004",
    name: "医薬品",
    colorCode: "#9C27B0",
    isCustom: false,
    isHidden: false,
  },
  {
    id: "22222222-2222-2222-2222-000000000005",
    name: "日用品",
    colorCode: "#4CAF50",
    isCustom: false,
    isHidden: false,
  },

  // ===== 削除済みカテゴリ（デモ用） =====
  {
    id: "deleted-item-category-001",
    name: "旧・冷凍食品",
    colorCode: "#9E9E9E",
    isCustom: true,
    isHidden: true,
  },
  {
    id: "deleted-item-category-002",
    name: "旧・アルコール類",
    colorCode: "#BDBDBD",
    isCustom: true,
    isHidden: true,
  },
];
