using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Infrastructure.Data.Entities
{
    /// <summary>
    /// ユーザーごとの給与項目カテゴリ（収入用）
    /// </summary>
    public class UserIncomeItemCategoryEntity : BaseEntity
    {
        /// <summary>
        /// 所属するユーザー設定ID
        /// </summary>
        public Guid UserSettingsId { get; set; }

        /// <summary>
        /// 元マスタID（カスタム項目の場合null）
        /// </summary>
        public Guid? MasterCategoryId { get; set; }

        /// <summary>
        /// 項目名（ユーザーが編集可能）
        /// </summary>
        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        /// <summary>
        /// 項目コード（LLM判定用）
        /// カスタム項目の場合は自動生成
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string Code { get; set; } = string.Empty;

        /// <summary>
        /// 表示色（ユーザーが編集可能）
        /// </summary>
        [Required]
        [MaxLength(7)]
        public string ColorCode { get; set; } = "#808080";

        /// <summary>
        /// 表示順序（ユーザーが並び替え可能）
        /// </summary>
        public int DisplayOrder { get; set; }

        /// <summary>
        /// ユーザー追加のカスタム項目か
        /// </summary>
        public bool IsCustom { get; set; }

        /// <summary>
        /// 非表示フラグ（削除されたが復元可能）
        /// </summary>
        public bool IsHidden { get; set; }

        /// <summary>
        /// ユーザー設定（ナビゲーションプロパティ）
        /// </summary>
        public UserSettingsEntity UserSettings { get; set; } = default!;

        /// <summary>
        /// 元マスタカテゴリ（ナビゲーションプロパティ）
        /// </summary>
        public IncomeItemCategoryMasterEntity? MasterCategory { get; set; }

        /// <summary>
        /// このカテゴリを使用している取引項目一覧
        /// </summary>
        public ICollection<TransactionItemEntity> TransactionItems { get; set; }
            = new List<TransactionItemEntity>();
    }
}
