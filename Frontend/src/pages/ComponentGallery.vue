<template>
  <!-- コンポーネントギャラリー仮置き -->
  <div class="p-8 space-y-4">
    <BaseButton variant="primary">プライマリ</BaseButton>
    <BaseButton variant="secondary">セカンダリ</BaseButton>
    <BaseButton variant="outline">アウトライン</BaseButton>
    <BaseButton variant="danger">危険</BaseButton>
    <BaseButton variant="text">テキスト</BaseButton>

    <BaseButton size="sm">Small</BaseButton>
    <BaseButton size="md">Medium</BaseButton>
    <BaseButton size="lg">Large</BaseButton>

    <BaseButton disabled>無効</BaseButton>
    <BaseButton full-width>全幅</BaseButton>
  </div>

  <div class="p-8 space-y-4 max-w-2xl">
    <h3 class="text-lg font-bold mb-4">バッジ</h3>
    <div class="flex items-center gap-3 flex-wrap">
      <BaseBadge color="default">Default</BaseBadge>
      <BaseBadge color="primary">Primary</BaseBadge>
      <BaseBadge color="success">Success</BaseBadge>
      <BaseBadge color="danger">Danger</BaseBadge>
      <BaseBadge color="warning">Warning</BaseBadge>
      <BaseBadge color="gray">Gray</BaseBadge>

      <BaseBadge size="sm" class="ml-4">SM</BaseBadge>
      <BaseBadge size="md">MD</BaseBadge>
      <BaseBadge size="lg">LG</BaseBadge>
    </div>

    <div class="mt-6">
      <BaseButton variant="primary" @click="isModalOpen = true"
        >モーダルを開く</BaseButton
      >
    </div>

    <BaseModal
      :isOpen="isModalOpen"
      title="サンプルモーダル"
      @close="isModalOpen = false"
    >
      <div class="space-y-4">
        <p>これはコンポーネントギャラリーのサンプルモーダルです。</p>
        <div class="flex gap-2">
          <BaseButton variant="secondary" @click="isModalOpen = false"
            >閉じる</BaseButton
          >
          <BaseButton variant="primary">承認</BaseButton>
        </div>
      </div>
    </BaseModal>
  </div>

  <div class="p-8 space-y-4 max-w-md">
    <BaseInput v-model="textValue" placeholder="テキスト入力" />
    <BaseInput v-model="emailValue" type="email" placeholder="メールアドレス" />
    <BaseInput v-model="dateValue" type="date" />

    <BaseInput placeholder="Small" size="sm" />
    <BaseInput placeholder="Medium" size="md" />
    <BaseInput placeholder="Large" size="lg" />

    <BaseInput placeholder="エラー状態" error />
    <BaseInput placeholder="無効" disabled />
    <BaseInput placeholder="読み取り専用" readonly value="編集不可" />
  </div>

  <div class="p-8 space-y-4 max-w-md">
    <div>
      <p class="text-sm text-gray-600 mb-2">空の状態</p>
      <BaseInputNumber v-model="amount1" placeholder="金額を入力" />
      <p class="text-xs text-gray-500 mt-1">値: {{ amount1 }}</p>
    </div>

    <div>
      <p class="text-sm text-gray-600 mb-2">初期値あり (1234567)</p>
      <BaseInputNumber v-model="amount2" />
      <p class="text-xs text-gray-500 mt-1">値: {{ amount2 }}</p>
    </div>

    <div>
      <p class="text-sm text-gray-600 mb-2">サイズ</p>
      <BaseInputNumber v-model="amount3" size="sm" placeholder="Small" />
      <BaseInputNumber
        v-model="amount3"
        size="md"
        placeholder="Medium"
        class="mt-2"
      />
      <BaseInputNumber
        v-model="amount3"
        size="lg"
        placeholder="Large"
        class="mt-2"
      />
    </div>

    <div>
      <p class="text-sm text-gray-600 mb-2">状態</p>
      <BaseInputNumber v-model="amount3" error placeholder="エラー状態" />
      <BaseInputNumber v-model="amount2" disabled class="mt-2" />
    </div>
  </div>
  <div class="p-8 space-y-4 max-w-2xl">
    <!-- 見出し -->
    <div>
      <BaseText variant="h1">見出し1（h1）</BaseText>
      <BaseText variant="h2">見出し2（h2）</BaseText>
      <BaseText variant="h3">見出し3（h3）</BaseText>
    </div>

    <!-- 本文・キャプション -->
    <div class="space-y-2">
      <BaseText variant="body"
        >本文テキストです。これが標準的なテキストサイズです。</BaseText
      >
      <BaseText variant="caption">キャプションテキスト（小さめ）</BaseText>
    </div>

    <!-- 金額表示 -->
    <div class="space-y-2">
      <BaseText variant="amount">¥123,456</BaseText>
      <BaseText variant="amount" color="success">+¥50,000（収入）</BaseText>
      <BaseText variant="amount" color="danger">-¥30,000（支出）</BaseText>
    </div>

    <!-- 色のバリエーション -->
    <div class="space-y-2">
      <BaseText color="default">デフォルト色</BaseText>
      <BaseText color="primary">プライマリ色（青）</BaseText>
      <BaseText color="success">成功色（緑・収入）</BaseText>
      <BaseText color="danger">危険色（赤・支出）</BaseText>
      <BaseText color="gray">グレー色（補足）</BaseText>
    </div>

    <!-- 太さのバリエーション -->
    <div class="space-y-2">
      <BaseText weight="normal">通常の太さ</BaseText>
      <BaseText weight="medium">中間の太さ</BaseText>
      <BaseText weight="bold">太字</BaseText>
    </div>

    <!-- カスタムタグ -->
    <div class="space-y-2">
      <BaseText variant="h1" as="div">divタグでh1スタイル</BaseText>
      <BaseText variant="body" as="label">labelタグでbodyスタイル</BaseText>
    </div>

    <!-- 組み合わせ -->
    <div class="space-y-2">
      <BaseText variant="h2" color="primary" weight="bold">
        カスタム見出し
      </BaseText>
      <BaseText variant="caption" color="gray" weight="medium">
        補足テキスト
      </BaseText>
    </div>
  </div>

  <div class="p-8 space-y-8 max-w-2xl">
    <!-- サイズバリエーション -->
    <div>
      <h3 class="text-lg font-bold mb-4">サイズ</h3>
      <div class="flex items-center gap-4">
        <BaseIcon name="home" size="sm" />
        <BaseIcon name="home" size="md" />
        <BaseIcon name="home" size="lg" />
        <BaseIcon name="home" size="xl" />
      </div>
    </div>

    <!-- アイコン一覧 -->
    <div>
      <h3 class="text-lg font-bold mb-4">アイコン一覧</h3>
      <div class="grid grid-cols-6 gap-4">
        <div class="flex flex-col items-center gap-2">
          <BaseIcon name="plus" />
          <span class="text-xs">plus</span>
        </div>
        <div class="flex flex-col items-center gap-2">
          <BaseIcon name="minus" />
          <span class="text-xs">minus</span>
        </div>
        <div class="flex flex-col items-center gap-2">
          <BaseIcon name="x" />
          <span class="text-xs">x</span>
        </div>
        <div class="flex flex-col items-center gap-2">
          <BaseIcon name="user" />
          <span class="text-xs">user</span>
        </div>
        <div class="flex flex-col items-center gap-2">
          <BaseIcon name="home" />
          <span class="text-xs">home</span>
        </div>
        <div class="flex flex-col items-center gap-2">
          <BaseIcon name="cart" />
          <span class="text-xs">cart</span>
        </div>
        <div class="flex flex-col items-center gap-2">
          <BaseIcon name="yen" />
          <span class="text-xs">yen</span>
        </div>
        <div class="flex flex-col items-center gap-2">
          <BaseIcon name="chart" />
          <span class="text-xs">chart</span>
        </div>
        <div class="flex flex-col items-center gap-2">
          <BaseIcon name="settings" />
          <span class="text-xs">settings</span>
        </div>
        <div class="flex flex-col items-center gap-2">
          <BaseIcon name="pencil" />
          <span class="text-xs">pencil</span>
        </div>
        <div class="flex flex-col items-center gap-2">
          <BaseIcon name="trash" />
          <span class="text-xs">trash</span>
        </div>
        <div class="flex flex-col items-center gap-2">
          <BaseIcon name="check" />
          <span class="text-xs">check</span>
        </div>
      </div>
    </div>

    <!-- ボタンと組み合わせ -->
    <div>
      <h3 class="text-lg font-bold mb-4">ボタンと組み合わせ</h3>
      <div class="flex gap-4">
        <BaseButton variant="primary">
          <BaseIcon name="plus" class="mr-2" />
          新規追加
        </BaseButton>
        <BaseButton variant="danger">
          <BaseIcon name="trash" class="mr-2" />
          削除
        </BaseButton>
        <BaseButton variant="secondary">
          <BaseIcon name="settings" class="mr-2" />
          設定
        </BaseButton>
      </div>
    </div>

    <!-- カラー付き -->
    <div>
      <h3 class="text-lg font-bold mb-4">カラー（Tailwindクラス追加）</h3>
      <div class="flex gap-4">
        <BaseIcon name="check" class="text-green-600" size="lg" />
        <BaseIcon name="warning" class="text-yellow-600" size="lg" />
        <BaseIcon name="info" class="text-blue-600" size="lg" />
        <BaseIcon name="x" class="text-red-600" size="lg" />
      </div>
    </div>
  </div>
  <div class="min-h-screen bg-gradient-to-br from-blue-50 to-indigo-100 p-8">
    <div class="max-w-4xl mx-auto space-y-8">
      <!-- ログインカード風 -->
      <BaseCard padding="lg">
        <BaseText variant="h2" class="mb-6 text-center">ログイン</BaseText>
        <div class="space-y-4">
          <BaseInput type="email" placeholder="メールアドレス" />
          <BaseInput type="password" placeholder="パスワード" />
          <BaseButton variant="primary" full-width>ログイン</BaseButton>
        </div>
      </BaseCard>

      <!-- ダッシュボード風カード -->
      <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
        <BaseCard>
          <BaseText variant="caption" color="gray" class="mb-2"
            >今月の支出</BaseText
          >
          <BaseText variant="amount" color="danger">¥123,456</BaseText>
        </BaseCard>

        <BaseCard>
          <BaseText variant="caption" color="gray" class="mb-2"
            >今月の収入</BaseText
          >
          <BaseText variant="amount" color="success">¥250,000</BaseText>
        </BaseCard>

        <BaseCard>
          <BaseText variant="caption" color="gray" class="mb-2">残高</BaseText>
          <BaseText variant="amount">¥126,544</BaseText>
        </BaseCard>
      </div>

      <!-- クリック可能なカード -->
      <div class="space-y-4">
        <BaseText variant="h3">最近の取引（クリック可能）</BaseText>

        <BaseCard clickable @click="handleCardClick">
          <div class="flex justify-between items-center">
            <div>
              <BaseText variant="body" weight="bold"
                >スーパーマーケット</BaseText
              >
              <BaseText variant="caption" color="gray">2024/01/15</BaseText>
            </div>
            <BaseText variant="body" color="danger" weight="bold"
              >-¥3,500</BaseText
            >
          </div>
        </BaseCard>

        <BaseCard clickable @click="handleCardClick">
          <div class="flex justify-between items-center">
            <div>
              <BaseText variant="body" weight="bold">給料</BaseText>
              <BaseText variant="caption" color="gray">2024/01/10</BaseText>
            </div>
            <BaseText variant="body" color="success" weight="bold"
              >+¥250,000</BaseText
            >
          </div>
        </BaseCard>

        <BaseCard clickable @click="handleCardClick">
          <div class="flex justify-between items-center">
            <div>
              <BaseText variant="body" weight="bold">電気代</BaseText>
              <BaseText variant="caption" color="gray">2024/01/05</BaseText>
            </div>
            <BaseText variant="body" color="danger" weight="bold"
              >-¥8,500</BaseText
            >
          </div>
        </BaseCard>
      </div>

      <!-- パディングのバリエーション -->
      <div class="space-y-4">
        <BaseText variant="h3">パディングのバリエーション</BaseText>

        <BaseCard padding="sm">
          <BaseText variant="body">Small Padding</BaseText>
        </BaseCard>

        <BaseCard padding="md">
          <BaseText variant="body">Medium Padding（デフォルト）</BaseText>
        </BaseCard>

        <BaseCard padding="lg">
          <BaseText variant="body">Large Padding</BaseText>
        </BaseCard>

        <BaseCard padding="none">
          <div class="p-4 bg-gray-100">
            <BaseText variant="body">Padding なし（中身で調整）</BaseText>
          </div>
        </BaseCard>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref } from "vue";
import BaseButton from "../components/atoms/BaseButton.vue";
import BaseInput from "../components/atoms/BaseInput.vue";
import BaseInputNumber from "../components/atoms/BaseInputNumber.vue";
import BaseText from "../components/atoms/BaseText.vue";
import BaseIcon from "../components/atoms/BaseIcon.vue";
import BaseCard from "../components/atoms/BaseCard.vue";
import BaseBadge from "../components/atoms/BaseBadge.vue";
import BaseModal from "../components/atoms/BaseModal.vue";

const textValue = ref("");
const emailValue = ref("");
const dateValue = ref("");

const amount1 = ref<number | null>(null);
const amount2 = ref<number | null>(1234567);
const amount3 = ref<number | null>(null);

const isModalOpen = ref(false);

const handleCardClick = () => {
  alert("カードがクリックされました");
};
</script>
