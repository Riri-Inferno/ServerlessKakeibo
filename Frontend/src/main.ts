import { createApp } from "vue";
import "./style.css";
import App from "./App.vue";
import router from "./router/index.ts";
import vue3GoogleLogin from "vue3-google-login";
import { useAuthStore } from "./stores/authStore";
import { createPinia } from "pinia";

const app = createApp(App);

// Pinia
const pinia = createPinia();
app.use(pinia);

// Google Login 初期化
app.use(vue3GoogleLogin, {
  clientId: import.meta.env.VITE_GOOGLE_CLIENT_ID,
});

app.use(router);
app.mount("#app");

// アプリ起動時に認証情報を復元
const authStore = useAuthStore();
authStore.initialize();
