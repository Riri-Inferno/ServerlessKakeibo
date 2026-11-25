import { createRouter, createWebHistory, createWebHashHistory } from "vue-router";
import { useAuthStore } from "../stores/authStore";
import { isDemoMode } from "../utils/env";
import ComponentGallery from "../pages/ComponentGallery.vue";
import Login from "../pages/Login.vue";
import Dashboard from "../pages/Dashboard.vue";
import TransactionList from "../pages/TransactionList.vue";
import Statistics from "../pages/Statistics.vue";
import Settings from "../pages/Settings.vue";
import AuthCallback from "../pages/AuthCallback.vue";
import OAuthConsent from "../pages/OAuthConsent.vue";
import { savePostLoginRedirect } from "../utils/postLoginRedirect";

const router = createRouter({
  // 環境に応じてモードを切り替え
  history: isDemoMode()
    ? createWebHashHistory(import.meta.env.BASE_URL)  // demo: ハッシュモード
    : createWebHistory(import.meta.env.BASE_URL),     // それ以外: ヒストリーモード
  routes: [
    {
      path: "/",
      name: "login",
      component: Login,
    },
    {
      path: "/auth/callback",
      name: "authCallback",
      component: AuthCallback,
      meta: { requiresAuth: false },
    },
    {
      path: "/dashboard",
      name: "dashboard",
      component: Dashboard,
      meta: { requiresAuth: true },
    },
    {
      path: "/transactions",
      name: "transactions",
      component: TransactionList,
      meta: { requiresAuth: true },
    },
    {
      path: "/component-gallery",
      name: "gallery",
      component: ComponentGallery,
    },
    {
      path: "/statistics",
      name: "stats",
      component: Statistics,
      meta: { requiresAuth: true },
    },
    {
      path: "/settings",
      name: "settings",
      component: Settings,
      meta: { requiresAuth: true },
    },
    {
      // OAuth の承認画面（backend の /oauth/authorize から転送されてくる）
      path: "/oauth/consent",
      name: "oauthConsent",
      component: OAuthConsent,
      meta: { requiresAuth: true, returnAfterLogin: true },
    },
  ],
});

router.beforeEach((to, _, next) => {
  const authStore = useAuthStore();

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    // 承認画面など、ログイン後に元の画面（パラメータ付き）へ戻す必要がある画面
    if (to.meta.returnAfterLogin) {
      savePostLoginRedirect(to.fullPath);
    }
    next({ name: "login" });
  } else if (to.name === "login" && authStore.isAuthenticated) {
    next({ name: "dashboard" });
  } else {
    next();
  }
});

export default router;
