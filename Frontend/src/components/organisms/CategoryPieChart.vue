<script setup lang="ts">
import { ref, watch, onMounted, onBeforeUnmount, nextTick } from "vue";
import {
  Chart as ChartJS,
  ArcElement,
  Tooltip,
  Legend,
  type ChartOptions,
  type ChartData,
} from "chart.js";
import ChartDataLabels from "chartjs-plugin-datalabels";
import type { CategorySummary } from "../../types/statistics";

ChartJS.register(ArcElement, Tooltip, Legend, ChartDataLabels);

interface Props {
  categories: CategorySummary[];
}

const props = defineProps<Props>();

const canvasRef = ref<HTMLCanvasElement | null>(null);
let chartInstance: ChartJS<"pie"> | null = null;

// レスポンシブなパディング設定
const getChartOptions = (): ChartOptions<"pie"> => {
  // スマホ判定
  const isMobile = window.innerWidth < 768;

  return {
    responsive: true,
    maintainAspectRatio: true,
    aspectRatio: 1.15,
    devicePixelRatio: 2,

    layout: {
      padding: isMobile
        ? {
            top: 20,
            right: 15,
            bottom: 45, // ラベル用のスペースのみ確保
            left: 15,
          }
        : {
            top: 30,
            right: 30,
            bottom: 60, // ラベル用のスペースのみ確保
            left: 30,
          },
    },

    plugins: {
      legend: {
        display: false,
      },
      tooltip: {
        callbacks: {
          label: (context) => {
            const value = context.parsed;
            return ` ¥${value.toLocaleString()}`;
          },
        },
      },
      datalabels: {
        color: "#374151",
        font: {
          weight: "bold",
          size: isMobile ? 10 : 12,
        },
        formatter: (value, context) => {
          const dataset = context.chart.data.datasets[0];
          if (!dataset || !dataset.data) return "";

          const total = (dataset.data as number[]).reduce(
            (sum, v) => sum + v,
            0,
          );
          const percentage = ((value / total) * 100).toFixed(1);

          const label = context.chart.data.labels![context.dataIndex];
          return `${label}\n${percentage}%`;
        },
        anchor: "end",
        align: "end",
        offset: isMobile ? 8 : 12,
        backgroundColor: "rgba(255, 255, 255, 0.95)",
        borderRadius: 4,
        padding: isMobile ? 4 : 6,
        borderWidth: 2,
        borderColor: (context) => {
          const bgColors = context.dataset.backgroundColor;
          if (!Array.isArray(bgColors)) {
            return "#6b7280";
          }
          return bgColors[context.dataIndex] as string;
        },
      },
    },
  };
};

// チャートデータを生成
const generateChartData = (): ChartData<"pie"> => {
  return {
    labels: props.categories.map((c) => c.categoryName),
    datasets: [
      {
        data: props.categories.map((c) => c.amount),
        backgroundColor: props.categories.map((c) => c.colorCode),
        borderWidth: 2,
        borderColor: "#ffffff",
      },
    ],
  };
};

// チャートを作成
const createChart = () => {
  if (!canvasRef.value) return;

  // 既存のインスタンスを破棄
  if (chartInstance) {
    chartInstance.destroy();
    chartInstance = null;
  }

  // 新しいインスタンスを作成
  chartInstance = new ChartJS(canvasRef.value, {
    type: "pie",
    data: generateChartData(),
    options: getChartOptions(),
  });
};

// チャートを更新
const updateChart = () => {
  if (!chartInstance) {
    createChart();
    return;
  }

  // データを更新
  chartInstance.data = generateChartData();
  chartInstance.options = getChartOptions();
  chartInstance.update("none");
};

// propsの変更を監視
watch(
  () => props.categories,
  async () => {
    // データが空の場合は破棄のみ
    if (!props.categories || props.categories.length === 0) {
      if (chartInstance) {
        chartInstance.destroy();
        chartInstance = null;
      }
      return;
    }

    // DOM更新を待ってからチャートを更新
    await nextTick();
    updateChart();
  },
  { deep: true, immediate: false },
);

// リサイズ時に再描画
let resizeTimeout: number | null = null;
const handleResize = () => {
  if (resizeTimeout) {
    clearTimeout(resizeTimeout);
  }
  resizeTimeout = window.setTimeout(() => {
    updateChart();
  }, 250);
};

// マウント時に作成
onMounted(async () => {
  if (props.categories && props.categories.length > 0) {
    await nextTick();
    createChart();
  }
  window.addEventListener("resize", handleResize);
});

// アンマウント時に破棄
onBeforeUnmount(() => {
  window.removeEventListener("resize", handleResize);
  if (resizeTimeout) {
    clearTimeout(resizeTimeout);
  }
  if (chartInstance) {
    chartInstance.destroy();
    chartInstance = null;
  }
});
</script>

<template>
  <div class="flex justify-center items-center w-full">
    <div class="w-full h-[350px] md:h-[480px] max-w-[400px] md:max-w-[500px]">
      <canvas ref="canvasRef"></canvas>
    </div>
  </div>
</template>
