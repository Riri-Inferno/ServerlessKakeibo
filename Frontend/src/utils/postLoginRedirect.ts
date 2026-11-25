/**
 * ログインしたあとに戻る先。
 * 未ログインで承認画面（/oauth/consent）に来た場合、ログイン後に元の画面（パラメータ付き）へ戻すために使う。
 * GitHub ログインはいったん別のサイトに移動するので、sessionStorage に保存する
 */
const STORAGE_KEY = "postLoginRedirect";

export const savePostLoginRedirect = (fullPath: string): void => {
  // 同じサイト内のパスだけを受け付ける（外部の URL に飛ばされないように）
  if (!fullPath.startsWith("/") || fullPath.startsWith("//")) return;
  sessionStorage.setItem(STORAGE_KEY, fullPath);
};

/** 保存した戻り先を取り出して消す。なければ null */
export const consumePostLoginRedirect = (): string | null => {
  const path = sessionStorage.getItem(STORAGE_KEY);
  sessionStorage.removeItem(STORAGE_KEY);
  return path && path.startsWith("/") && !path.startsWith("//") ? path : null;
};
