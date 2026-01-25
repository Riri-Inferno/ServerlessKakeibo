/**
 * API ステータス
 */
export const ApiStatus = {
  Success: "Success",
  Fail: "Fail",
  InvalidRequest: "InvalidRequest",
  Unauthorized: "Unauthorized",
  NotFound: "NotFound",
  InternalError: "InternalError",
} as const;

export type ApiStatus = (typeof ApiStatus)[keyof typeof ApiStatus];

/**
 * API レスポンスの基本形
 */
export interface ApiResponse<T> {
  status: ApiStatus;
  message: string | null;
  data: T;
}
