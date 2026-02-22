export const isDemoMode = (): boolean => {
  return import.meta.env.VITE_ENVIRONMENT === "demo";
};
