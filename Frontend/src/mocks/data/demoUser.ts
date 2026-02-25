import type { LoginResult } from "../../types/auth";

/**
 * デモログイン用のモックユーザー
 */
export const mockDemoUser: LoginResult = {
  accessToken: "demo-access-token-xxxxxxxxxxxxxxxxxxxxxxxx",
  refreshToken: "demo-refresh-token-xxxxxxxxxxxxxxxxxxxxxxxx",
  userId: "00000000-0000-0000-0000-000000000000",
  displayName: "デモユーザー",
  pictureUrl: `${import.meta.env.BASE_URL}demo-user.png`,
};
