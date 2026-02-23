/**
 * デモ用の収入項目カテゴリマスターデータ（収入用・明細レベル）
 * isHidden: false = 表示中、true = 削除済み
 */
export const mockIncomeItemCategories = [
  // ===== システムデフォルト =====
  {
    id: "33333333-3333-3333-3333-000000000001",
    name: "基本給",
    colorCode: "#4CAF50",
    isCustom: false,
    isHidden: false,
  },
  {
    id: "33333333-3333-3333-3333-000000000002",
    name: "残業代",
    colorCode: "#FF9800",
    isCustom: false,
    isHidden: false,
  },
  {
    id: "33333333-3333-3333-3333-000000000003",
    name: "賞与",
    colorCode: "#FFD700",
    isCustom: false,
    isHidden: false,
  },
  {
    id: "33333333-3333-3333-3333-000000000004",
    name: "交通費",
    colorCode: "#2196F3",
    isCustom: false,
    isHidden: false,
  },
  {
    id: "33333333-3333-3333-3333-000000000005",
    name: "手当",
    colorCode: "#9C27B0",
    isCustom: false,
    isHidden: false,
  },

  // ===== カスタム（実データから） =====
  {
    id: "385c94d5-bbad-453d-9967-57b264030d3a",
    name: "業務委託料",
    colorCode: "#66BB6A",
    isCustom: true,
    isHidden: false,
  },

  // ===== 削除済みカテゴリ（デモ用） =====
  {
    id: "deleted-income-item-001",
    name: "旧・住宅手当",
    colorCode: "#9E9E9E",
    isCustom: true,
    isHidden: true,
  },
  {
    id: "deleted-income-item-002",
    name: "旧・資格手当",
    colorCode: "#BDBDBD",
    isCustom: true,
    isHidden: true,
  },
];
