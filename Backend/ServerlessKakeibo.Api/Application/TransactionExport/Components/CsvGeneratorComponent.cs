using System.Globalization;
using System.Text;
using CsvHelper;
using CsvHelper.Configuration;

namespace ServerlessKakeibo.Api.Application.TransactionExport.Components;

/// <summary>
/// CSV生成コンポーネント
/// </summary>
public class CsvGeneratorComponent
{
    /// <summary>
    /// CSV生成（UTF-8 BOM付き）
    /// </summary>
    /// <typeparam name="T">レコード型</typeparam>
    /// <param name="records">レコードリスト</param>
    /// <returns>CSVバイナリデータ</returns>
    public static byte[] GenerateCsv<T>(IEnumerable<T> records) where T : class
    {
        if (records == null)
            throw new ArgumentNullException(nameof(records));

        using var memoryStream = new MemoryStream();

        // UTF-8 BOM付きエンコーディング
        var encoding = new UTF8Encoding(true);

        using (var writer = new StreamWriter(memoryStream, encoding, leaveOpen: true))
        using (var csv = new CsvWriter(writer, new CsvConfiguration(CultureInfo.InvariantCulture)
        {
            // ヘッダーをプロパティ名そのまま使用（日本語カラム名）
            HasHeaderRecord = true,

            // フィールドをダブルクォートで囲む（Excel対策）
            ShouldQuote = args => true
        }))
        {
            csv.WriteRecords(records);
        }

        return memoryStream.ToArray();
    }
}
