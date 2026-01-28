using ServerlessKakeibo.Api.Application.UserData.Dto;

namespace ServerlessKakeibo.Api.Application.UserData.Usecases
{
    /// <summary>
    /// ユーザーの全取引データ削除ユースケース
    /// </summary>
    public interface IDeleteAllTransactionsUseCase
    {
        /// <summary>
        /// ユーザーに紐づく全取引データを論理削除
        /// </summary>
        /// <param name="userId">ユーザーID</param>
        /// <param name="cancellationToken">キャンセルトークン</param>
        /// <returns>削除結果</returns>
        Task<DeleteAllTransactionsResult> ExecuteAsync(
            Guid userId,
            CancellationToken cancellationToken = default);
    }
}
