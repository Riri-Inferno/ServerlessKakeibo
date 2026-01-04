using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Infrastructure.Data.Entities
{
    /// <summary>
    /// すべての永続化エンティティが継承する基底クラス
    /// </summary>
    public abstract class BaseEntity
    {
        /// <summary>
        /// 主キー (自動採番)
        /// </summary>
        public Guid Id { get; set; }

        /// <summary>
        /// レコード作成日時
        /// </summary>
        public DateTimeOffset CreatedAt { get; set; }

        /// <summary>
        /// レコード作成者のユーザーID
        /// </summary>
        public Guid CreatedBy { get; set; }

        /// <summary>
        /// レコードの最終更新日時（作成時にも設定される）
        /// </summary>
        public DateTimeOffset UpdatedAt { get; set; }

        /// <summary>
        /// 最終更新者のユーザーID
        /// </summary>
        public Guid? UpdatedBy { get; set; }

        /// <summary>
        /// 論理削除フラグ（true の場合は削除扱い）
        /// </summary>
        public bool IsDeleted { get; set; }

        /// <summary>
        /// テナントID（家計簿などのデータ所属単位）
        /// 将来的なマルチテナント対応可能性のため
        /// </summary>
        public Guid TenantId { get; set; }

        /// <summary>
        /// 楽観ロック用のバージョン管理（PostgreSQLのxminを使用）
        /// </summary>
        public uint RowVersion { get; set; }
    }
}
