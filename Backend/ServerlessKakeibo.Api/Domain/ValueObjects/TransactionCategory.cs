using System.ComponentModel;

namespace ServerlessKakeibo.Api.Domain.ValueObjects;

/// <summary>
/// 取引カテゴリ
/// </summary>
public enum TransactionCategory
{
    #region Expense Categories
    /// <summary>
    /// 未分類
    /// </summary>
    [Description("未分類")]
    Uncategorized = 0,

    /// <summary>
    /// 食費
    /// </summary>
    [Description("食費")]
    Food = 1,

    /// <summary>
    /// 外食
    /// </summary>
    [Description("外食")]
    DiningOut = 2,

    /// <summary>
    /// 日用品
    /// </summary>
    [Description("日用品")]
    DailyNecessities = 3,

    /// <summary>
    /// 交通費
    /// </summary>
    [Description("交通費")]
    Transportation = 4,

    /// <summary>
    /// 教育・教養
    /// </summary>
    [Description("教育・教養")]
    Education = 5,

    /// <summary>
    /// 医療・健康
    /// </summary>
    [Description("医療・健康")]
    Medical = 6,

    /// <summary>
    /// 趣味・娯楽
    /// </summary>
    [Description("趣味・娯楽")]
    Entertainment = 7,

    /// <summary>
    /// 衣服・美容
    /// </summary>
    [Description("衣服・美容")]
    Fashion = 8,

    /// <summary>
    /// 水道・光熱費
    /// </summary>
    [Description("水道・光熱費")]
    Utilities = 9,

    /// <summary>
    /// 通信費
    /// </summary>
    [Description("通信費")]
    Communication = 10,

    /// <summary>
    /// その他
    /// </summary>
    [Description("その他")]
    Other = 99,
    #endregion

    #region Income Categories
    /// <summary>
    /// 給与
    /// </summary>
    [Description("給与")]
    Salary = 100,

    /// <summary>
    /// その他収入
    /// </summary>
    [Description("その他収入")]
    OtherIncome = 101
    #endregion
}

/// <summary>
/// TransactionCategory 拡張メソッド
/// </summary>
public static class TransactionCategoryExtensions
{
    /// <summary>
    /// 日本語名を取得
    /// </summary>
    public static string ToJapanese(this TransactionCategory category)
    {
        var type = typeof(TransactionCategory);
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
    public static TransactionCategory? FromJapanese(string? categoryName)
    {
        if (string.IsNullOrWhiteSpace(categoryName))
            return null;

        return categoryName switch
        {
            "食費" => TransactionCategory.Food,
            "外食" => TransactionCategory.DiningOut,
            "日用品" => TransactionCategory.DailyNecessities,
            "交通費" => TransactionCategory.Transportation,
            "教育・教養" => TransactionCategory.Education,
            "医療・健康" => TransactionCategory.Medical,
            "趣味・娯楽" => TransactionCategory.Entertainment,
            "衣服・美容" => TransactionCategory.Fashion,
            "水道・光熱費" => TransactionCategory.Utilities,
            "通信費" => TransactionCategory.Communication,
            "その他" => TransactionCategory.Other,
            "未分類" => TransactionCategory.Uncategorized,
            "給与" => TransactionCategory.Salary,
            "その他収入" => TransactionCategory.OtherIncome,
            _ => null
        };
    }
}
