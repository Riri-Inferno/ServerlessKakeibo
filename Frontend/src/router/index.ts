import { createRouter, createWebHistory } from "vue-router";
import ComponentGallery from "../pages/ComponentGallery.vue";
import Login from "../pages/Login.vue";

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/",
      name: "login",
      component: Login,
    },
    {
      path: "/component-gallery",
      name: "gallery",
      component: ComponentGallery,
    },
  ],
});

export default router;
