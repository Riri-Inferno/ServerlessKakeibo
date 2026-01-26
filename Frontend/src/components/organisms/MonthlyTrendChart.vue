<!-- src/components/organisms/MonthlyTrendChart.vue -->
<script setup lang="ts">
import { computed } from "vue";
import { Line } from "vue-chartjs";
import {
  Chart as ChartJS,
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
  type ChartOptions,
} from "chart.js";
import type { MonthlyTrendResult } from "../../types/statistics";

ChartJS.register(
  CategoryScale,
  LinearScale,
  PointElement,
  LineElement,
  Title,
  Tooltip,
  Legend,
);

interface Props {
  trend: MonthlyTrendResult;
}

const props = defineProps<Props>();

const chartData = computed(() => ({
  labels: props.trend.months.map((m) => m.label),
  datasets: [
    {
      label: "収入",
      data: props.trend.incomes,
      borderColor: "rgb(34, 197, 94)",
      backgroundColor: "rgba(34, 197, 94, 0.1)",
      tension: 0.3,
    },
    {
      label: "支出",
      data: props.trend.expenses,
      borderColor: "rgb(239, 68, 68)",
      backgroundColor: "rgba(239, 68, 68, 0.1)",
      tension: 0.3,
    },
  ],
}));

const chartOptions: ChartOptions<"line"> = {
  responsive: true,
  maintainAspectRatio: true,
  plugins: {
    legend: {
      position: "bottom",
    },
    tooltip: {
      callbacks: {
        label: (context) => {
          const value = context.parsed.y ?? 0;
          return `${context.dataset.label}: ¥${value.toLocaleString()}`;
        },
      },
    },
  },
  scales: {
    y: {
      beginAtZero: true,
      ticks: {
        callback: (value) => `¥${Number(value).toLocaleString()}`,
      },
    },
  },
};
</script>

<template>
  <div class="w-full">
    <Line :data="chartData" :options="chartOptions" />
  </div>
</template>
