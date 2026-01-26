namespace ServerlessKakeibo.Api.Common.Helpers;

/// <summary>
/// 数値計算ヘルパー
/// </summary>
public static class MathHelper
{
    /// <summary>
    /// パーセンテージ変化を計算
    /// </summary>
    /// <param name="oldValue">旧値</param>
    /// <param name="newValue">新値</param>
    /// <param name="decimalPlaces">小数点以下の桁数（デフォルト: 2）</param>
    /// <returns>
    /// パーセンテージ変化。
    /// 旧値が0の場合：新値が正なら100、負なら-100、0なら0を返す。
    /// </returns>
    /// <example>
    /// <code>
    /// MathHelper.CalculatePercentChange(100, 150);  // 50.00
    /// MathHelper.CalculatePercentChange(100, 80);   // -20.00
    /// MathHelper.CalculatePercentChange(0, 100);    // 100
    /// MathHelper.CalculatePercentChange(0, 0);      // 0
    /// </code>
    /// </example>
    public static decimal? CalculatePercentChange(
        decimal oldValue,
        decimal newValue,
        int decimalPlaces = 2)
    {
        if (oldValue == 0)
        {
            if (newValue > 0) return 100;
            if (newValue < 0) return -100;
            return 0;
        }

        var change = ((newValue - oldValue) / oldValue) * 100;
        return Math.Round(change, decimalPlaces);
    }

    /// <summary>
    /// 安全な除算（ゼロ除算対策）
    /// </summary>
    /// <param name="numerator">分子</param>
    /// <param name="denominator">分母</param>
    /// <param name="defaultValue">分母が0の場合の戻り値（デフォルト: 0）</param>
    /// <returns>除算結果</returns>
    public static decimal SafeDivide(
        decimal numerator,
        decimal denominator,
        decimal defaultValue = 0)
    {
        return denominator == 0 ? defaultValue : numerator / denominator;
    }

    /// <summary>
    /// 割合を計算（パーセンテージ）
    /// </summary>
    /// <param name="part">部分値</param>
    /// <param name="total">全体値</param>
    /// <param name="decimalPlaces">小数点以下の桁数（デフォルト: 2）</param>
    /// <returns>割合（%）。全体値が0の場合は0を返す。</returns>
    /// <example>
    /// <code>
    /// MathHelper.CalculatePercentage(25, 100);  // 25.00
    /// MathHelper.CalculatePercentage(1, 3);     // 33.33
    /// </code>
    /// </example>
    public static decimal CalculatePercentage(
        decimal part,
        decimal total,
        int decimalPlaces = 2)
    {
        if (total == 0) return 0;

        var percentage = (part / total) * 100;
        return Math.Round(percentage, decimalPlaces);
    }
}
