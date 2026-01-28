namespace ServerlessKakeibo.Api.Application.UserData.Dto
{
    /// <summary>
    /// 全取引データ削除結果
    /// </summary>
    public class DeleteAllTransactionsResult
    {
        /// <summary>
        /// 削除された取引数
        /// </summary>
        public int DeletedTransactionCount { get; set; }

        /// <summary>
        /// 削除された取引明細数
        /// </summary>
        public int DeletedTransactionItemCount { get; set; }

        /// <summary>
        /// 削除された店舗詳細数
        /// </summary>
        public int DeletedShopDetailCount { get; set; }

        /// <summary>
        /// 削除された税詳細数
        /// </summary>
        public int DeletedTaxDetailCount { get; set; }

        /// <summary>
        /// 削除されたGCS画像数
        /// </summary>
        public int DeletedImageCount { get; set; }

        /// <summary>
        /// 削除に失敗したGCS画像数
        /// </summary>
        public int FailedImageCount { get; set; }

        /// <summary>
        /// 削除に失敗した画像のパスリスト
        /// </summary>
        public List<string> FailedImagePaths { get; set; } = new();

        /// <summary>
        /// 削除処理完了日時
        /// </summary>
        public DateTimeOffset CompletedAt { get; set; }
    }
}
