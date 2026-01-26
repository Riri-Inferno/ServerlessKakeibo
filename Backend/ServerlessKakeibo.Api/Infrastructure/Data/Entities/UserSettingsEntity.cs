using System.ComponentModel.DataAnnotations;

namespace ServerlessKakeibo.Api.Infrastructure.Data.Entities
{
    /// <summary>
    /// ユーザー設定エンティティ(家計簿の個人設定)
    /// </summary>
    public class UserSettingsEntity : BaseEntity
    {
        /// <summary>
        /// 設定の所有者であるユーザーID
        /// </summary>
        public Guid UserId { get; set; }

        /// <summary>
        /// 締め日(1〜31)
        /// nullの場合は月末締め(各月の最終日)
        /// 例: 25なら毎月25日締め、翌26日が新しい期間の開始
        /// </summary>
        [Range(1, 31)]
        public int? ClosingDay { get; set; }

        /// <summary>
        /// タイムゾーン(IANA形式, 例: "Asia/Tokyo")
        /// デフォルトはAsia/Tokyo
        /// </summary>
        [Required]
        [MaxLength(50)]
        public string TimeZone { get; set; } = "Asia/Tokyo";

        /// <summary>
        /// 通貨コード(ISO 4217, 例: "JPY")
        /// 将来的な多通貨対応のため
        /// </summary>
        [Required]
        [MaxLength(3)]
        public string CurrencyCode { get; set; } = "JPY";

        /// <summary>
        /// 表示名の上書き
        /// - null: UserEntity.DisplayName(Google由来)を使用
        /// - 値あり: この値を優先表示
        /// 
        /// Googleログイン時はUserEntity.DisplayNameが更新されるが、
        /// この値が設定されている場合は画面表示でこちらを優先する
        /// </summary>
        [MaxLength(100)]
        public string? DisplayNameOverride { get; set; }

        /// <summary>
        /// ユーザーエンティティへのナビゲーションプロパティ
        /// </summary>
        public UserEntity User { get; set; } = null!;
    }
}
