using System.Text.Json.Serialization;
using ServerlessKakeibo.Api.Contracts.Enums;

namespace ServerlessKakeibo.Api.Contracts;

/// <summary>
/// API レスポンスの基本形
/// </summary>
/// <typeparam name="T">レスポンスデータの型</typeparam>
public class ApiResponse<T>
{
    /// <summary>
    /// API ステータス
    /// </summary>
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public ApiStatus Status { get; set; }

    /// <summary>
    /// エラーメッセージ
    /// </summary>
    public string? Message { get; set; } = null;

    /// <summary>
    /// データ本体
    /// Dtoなどを格納
    /// </summary>
    public T? Data { get; set; }

    /// <summary>
    /// 成功レスポンスを生成
    /// </summary>
    /// <param name="data"></param>
    /// <returns></returns>
    public static ApiResponse<T> Success(T data)
        => new() { Status = ApiStatus.Success, Data = data };

    /// <summary>
    /// 失敗レスポンスを生成
    /// </summary>
    /// <param name="status"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    public static ApiResponse<T> Fail(ApiStatus status, string? message = null)
        => new() { Status = status, Message = message };
}
