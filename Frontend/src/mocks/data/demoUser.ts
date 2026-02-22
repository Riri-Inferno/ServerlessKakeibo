import type { LoginResult } from "../../types/auth";

/**
 * デモログイン用のモックユーザー
 */
export const mockDemoUser: LoginResult = {
  accessToken: "demo-access-token-xxxxxxxxxxxxxxxxxxxxxxxx",
  refreshToken: "demo-refresh-token-xxxxxxxxxxxxxxxxxxxxxxxx",
  userId: "00000000-0000-0000-0000-000000000000",
  displayName: "デモユーザー",
  pictureUrl: "", // アバター画像なし（デフォルト表示）
};
