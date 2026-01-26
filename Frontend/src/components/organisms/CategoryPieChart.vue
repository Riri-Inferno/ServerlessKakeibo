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
import type { CategorySummary } from "../../types/statistics";

ChartJS.register(ArcElement, Tooltip, Legend);

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
  plugins: {
    legend: {
      display: false, // カスタムリストを使うため非表示
    },
    tooltip: {
      callbacks: {
        label: (context) => {
          const value = context.parsed;
          return ` ¥${value.toLocaleString()}`;
        },
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
