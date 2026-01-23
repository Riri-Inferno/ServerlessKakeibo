namespace ServerlessKakeibo.Api.Service.Models;

/// <summary>
/// GCSバッチ削除結果
/// </summary>
public class GcsBatchDeleteResult
{
    // プロパティからフィールドに変更（Interlockedで使用するため）
    private int _successCount;
    private int _failureCount;

    /// <summary>
    /// 削除成功数
    /// </summary>
    public int SuccessCount
    {
        get => _successCount;
        set => _successCount = value;
    }

    /// <summary>
    /// 削除失敗数
    /// </summary>
    public int FailureCount
    {
        get => _failureCount;
        set => _failureCount = value;
    }

    /// <summary>
    /// エラー詳細リスト
    /// </summary>
    public List<GcsDeleteError> Errors { get; set; } = new();

    /// <summary>
    /// すべて成功したか
    /// </summary>
    public bool IsAllSuccess => FailureCount == 0;

    /// <summary>
    /// 成功数をスレッドセーフにインクリメント
    /// </summary>
    internal void IncrementSuccess() => Interlocked.Increment(ref _successCount);

    /// <summary>
    /// 失敗数をスレッドセーフにインクリメント
    /// </summary>
    internal void IncrementFailure() => Interlocked.Increment(ref _failureCount);
}

/// <summary>
/// GCS削除エラー情報
/// </summary>
public class GcsDeleteError
{
    /// <summary>
    /// エラーが発生したオブジェクトパス
    /// </summary>
    public string ObjectPath { get; set; } = string.Empty;

    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string ErrorMessage { get; set; } = string.Empty;

    /// <summary>
    /// HTTPステータスコード（存在する場合）
    /// </summary>
    public int? StatusCode { get; set; }
}
