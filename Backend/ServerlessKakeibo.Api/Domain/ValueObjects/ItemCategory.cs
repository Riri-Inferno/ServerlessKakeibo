using System.ComponentModel;

namespace ServerlessKakeibo.Api.Domain.ValueObjects;

/// <summary>
/// 商品カテゴリ
/// </summary>
public enum ItemCategory
{
    /// <summary>
    /// 未分類
    /// </summary>
    [Description("未分類")]
    Uncategorized = 0,

    // === 食品・飲料 ===
    [Description("食品")]
    Food = 1,

    [Description("飲料")]
    Beverage = 2,

    [Description("お菓子・スナック")]
    Snack = 3,

    [Description("冷凍食品")]
    FrozenFood = 4,

    [Description("乳製品")]
    DairyProduct = 5,

    [Description("調味料")]
    Seasoning = 6,

    // === 日用品 ===
    [Description("トイレタリー")]
    Toiletries = 10,

    [Description("キッチン用品")]
    KitchenSupplies = 11,

    [Description("掃除用品")]
    CleaningSupplies = 12,

    [Description("洗濯用品")]
    LaundrySupplies = 13,

    // === 文具・雑貨 ===
    [Description("文房具")]
    Stationery = 20,

    [Description("雑貨")]
    Miscellaneous = 21,

    // === 医薬品・化粧品 ===
    [Description("医薬品")]
    Medicine = 30,

    [Description("サプリメント")]
    Supplement = 31,

    [Description("化粧品")]
    Cosmetics = 32,

    // === 衣類・ファッション ===
    [Description("衣類")]
    Clothing = 40,

    [Description("靴")]
    Shoes = 41,

    [Description("アクセサリー")]
    Accessories = 42,

    // === 電子機器 ===
    [Description("電子機器")]
    Electronics = 50,

    [Description("電池")]
    Battery = 51,

    // === その他 ===
    [Description("ペット用品")]
    PetSupplies = 60,

    [Description("ベビー用品")]
    BabyProducts = 61,

    [Description("レジ袋・包装材")]
    Packaging = 62,

    [Description("タバコ")]
    Tobacco = 63,

    [Description("書籍・雑誌")]
    Books = 64,

    [Description("その他")]
    Other = 99
}

/// <summary>
/// ItemCategory 拡張メソッド
/// </summary>
public static class ItemCategoryExtensions
{
    /// <summary>
    /// 日本語名を取得
    /// </summary>
    public static string ToJapanese(this ItemCategory category)
    {
        var type = typeof(ItemCategory);
        var memberInfo = type.GetMember(category.ToString());

        if (memberInfo.Length > 0)
        {
            var attributes = memberInfo[0].GetCustomAttributes(typeof(DescriptionAttribute), false);
            if (attributes.Length > 0)
            {
                return ((DescriptionAttribute)attributes[0]).Description;
            }
        }

        return category.ToString();
    }

    /// <summary>
    /// 日本語名からEnumに変換
    /// </summary>
    public static ItemCategory? FromJapanese(string? categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return null;

        return categoryName switch
        {
            // 食品・飲料
            "食品" => ItemCategory.Food,
            "飲料" => ItemCategory.Beverage,
            "お菓子・スナック" => ItemCategory.Snack,
            "冷凍食品" => ItemCategory.FrozenFood,
            "乳製品" => ItemCategory.DairyProduct,
            "調味料" => ItemCategory.Seasoning,

            // 日用品
            "トイレタリー" => ItemCategory.Toiletries,
            "キッチン用品" => ItemCategory.KitchenSupplies,
            "掃除用品" => ItemCategory.CleaningSupplies,
            "洗濯用品" => ItemCategory.LaundrySupplies,

            // 文具・雑貨
            "文房具" => ItemCategory.Stationery,
            "雑貨" => ItemCategory.Miscellaneous,

            // 医薬品・化粧品
            "医薬品" => ItemCategory.Medicine,
            "サプリメント" => ItemCategory.Supplement,
            "化粧品" => ItemCategory.Cosmetics,

            // 衣類・ファッション
            "衣類" => ItemCategory.Clothing,
            "靴" => ItemCategory.Shoes,
            "アクセサリー" => ItemCategory.Accessories,

            // 電子機器
            "電子機器" => ItemCategory.Electronics,
            "電池" => ItemCategory.Battery,

            // その他
            "ペット用品" => ItemCategory.PetSupplies,
            "ベビー用品" => ItemCategory.BabyProducts,
            "レジ袋・包装材" => ItemCategory.Packaging,
            "タバコ" => ItemCategory.Tobacco,
            "書籍・雑誌" => ItemCategory.Books,
            "その他" => ItemCategory.Other,
            "未分類" => ItemCategory.Uncategorized,

            _ => null
        };
    }
}
