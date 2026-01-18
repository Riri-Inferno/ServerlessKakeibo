import { createRouter, createWebHistory } from "vue-router";
import ComponentGallery from "../pages/ComponentGallery.vue";

const router = createRouter({
  history: createWebHistory(),
  routes: [
    {
      path: "/component-gallery",
      name: "gallery",
      component: ComponentGallery,
    },
  ],
});

export default router;
