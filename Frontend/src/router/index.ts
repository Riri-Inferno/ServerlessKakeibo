import { createRouter, createWebHistory } from "vue-router";
import { useAuthStore } from "../stores/authStore";
import ComponentGallery from "../pages/ComponentGallery.vue";
import Login from "../pages/Login.vue";
import Dashboard from "../pages/Dashboard.vue";

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/",
      name: "login",
      component: Login,
    },
    {
      path: "/dashboard",
      name: "dashboard",
      component: Dashboard,
      meta: { requiresAuth: true },
    },
    {
      path: "/component-gallery",
      name: "gallery",
      component: ComponentGallery,
    },
  ],
});

router.beforeEach((to, from, next) => {
  const authStore = useAuthStore();
  // void from;

  if (to.meta.requiresAuth && !authStore.isAuthenticated) {
    next({ name: "login" });
  } else if (to.name === "login" && authStore.isAuthenticated) {
    next({ name: "dashboard" });
  } else {
    next();
  }
});

export default router;
