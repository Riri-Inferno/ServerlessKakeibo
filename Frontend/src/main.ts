import { createApp } from "vue";
import "./style.css";
import App from "./App.vue";
import router from "./router/index.ts";
import vue3GoogleLogin from "vue3-google-login";

const app = createApp(App);

// Google Login 初期化
app.use(vue3GoogleLogin, {
  clientId: import.meta.env.VITE_GOOGLE_CLIENT_ID,
});

app.use(router);
app.mount("#app");
