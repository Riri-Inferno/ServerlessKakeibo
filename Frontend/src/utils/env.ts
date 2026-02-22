/**
 * デモモード判定
 * window.ENV.ENVIRONMENT が優先、なければ import.meta.env.VITE_ENVIRONMENT を使用
 */
export const isDemoMode = (): boolean => {
  const env =
    (window as any).ENV?.ENVIRONMENT ||
    import.meta.env.VITE_ENVIRONMENT ||
    "";
  return env === "demo";
};
