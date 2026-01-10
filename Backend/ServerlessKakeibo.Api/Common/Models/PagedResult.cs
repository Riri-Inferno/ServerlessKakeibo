namespace ServerlessKakeibo.Api.Common.Models;

/// <summary>
/// ページング結果
/// </summary>
/// <typeparam name="T">データ型</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// データリスト
    /// </summary>
    public List<T> Items { get; set; } = new();

    /// <summary>
    /// 現在のページ番号(1始まり)
    /// </summary>
    public int CurrentPage { get; set; }

    /// <summary>
    /// 1ページあたりの件数
    /// </summary>
    public int PageSize { get; set; }

    /// <summary>
    /// 総件数
    /// </summary>
    public int TotalCount { get; set; }

    /// <summary>
    /// 総ページ数
    /// </summary>
    public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>
    /// 前のページが存在するか
    /// </summary>
    public bool HasPreviousPage => CurrentPage > 1;

    /// <summary>
    /// 次のページが存在するか
    /// </summary>
    public bool HasNextPage => CurrentPage < TotalPages;
}
