<script setup lang="ts">
import { computed } from "vue";
import { Pie } from "vue-chartjs";
import {
  Chart as ChartJS,
  ArcElement,
  Tooltip,
  Legend,
  type ChartOptions,
} from "chart.js";
import ChartDataLabels from "chartjs-plugin-datalabels";
import type { CategorySummary } from "../../types/statistics";

ChartJS.register(ArcElement, Tooltip, Legend, ChartDataLabels);

interface Props {
  categories: CategorySummary[];
}

const props = defineProps<Props>();

const chartData = computed(() => {
  const getCategoryColor = (index: number) => {
    const hue = index * 137.5;
    return `hsl(${hue % 360}, 70%, 50%)`;
  };

  return {
    labels: props.categories.map((c) => c.categoryName),
    datasets: [
      {
        data: props.categories.map((c) => c.amount),
        backgroundColor: props.categories.map((_, i) => getCategoryColor(i)),
        borderWidth: 2,
        borderColor: "#ffffff",
      },
    ],
  };
});

const chartOptions: ChartOptions<"pie"> = {
  responsive: true,
  maintainAspectRatio: true,
  aspectRatio: 1,

  layout: {
    padding: {
      top: 60,
      right: 60,
      bottom: 20,
      left: 60,
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
        size: 12,
      },
      formatter: (value, context) => {
        const dataset = context.chart.data.datasets[0];
        if (!dataset || !dataset.data) return "";

        const total = (dataset.data as number[]).reduce((sum, v) => sum + v, 0);
        const percentage = ((value / total) * 100).toFixed(1);

        const label = context.chart.data.labels![context.dataIndex];
        return `${label}\n${percentage}%`;
      },
      anchor: "end",
      align: "end",
      offset: 10,
      backgroundColor: "rgba(255, 255, 255, 0.95)",
      borderRadius: 4,
      padding: 6,
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
</script>

<template>
  <div class="w-full" style="max-width: 400px; margin: 0 auto">
    <Pie :data="chartData" :options="chartOptions" />
  </div>
</template>
