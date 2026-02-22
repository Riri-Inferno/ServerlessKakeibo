import { createApp } from "vue";
import "./style.css";
import App from "./App.vue";
import router from "./router/index.ts";
import vue3GoogleLogin from "vue3-google-login";
import { useAuthStore } from "./stores/authStore";
import { createPinia } from "pinia";

// チャート自動登録
import "chart.js/auto";

// datalabelsは明示登録が必要
import { Chart } from "chart.js";
import ChartDataLabels from "chartjs-plugin-datalabels";

Chart.register(ChartDataLabels);

const app = createApp(App);

// Pinia
const pinia = createPinia();
app.use(pinia);

// window.ENVを優先（Docker環境）、フォールバックでimport.meta.env
const googleClientId =
  (window as any).ENV?.GOOGLE_CLIENT_ID ||
  import.meta.env.VITE_GOOGLE_CLIENT_ID ||
  "";

// Google Login 初期化
app.use(vue3GoogleLogin, {
  clientId: googleClientId,
});

app.use(router);
app.mount("#app");

// アプリ起動時に認証情報を復元
const authStore = useAuthStore();
authStore.initialize();
