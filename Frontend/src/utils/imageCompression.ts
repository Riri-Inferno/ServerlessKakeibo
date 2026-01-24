import imageCompression from "browser-image-compression";

/**
 * 画像を圧縮する
 *
 * @param file 元の画像ファイル
 * @param options 圧縮オプション
 * @returns 圧縮後のファイル
 */
export async function compressImage(
  file: File,
  options?: {
    maxSizeMB?: number;
    maxWidthOrHeight?: number;
    useWebWorker?: boolean;
  },
): Promise<File> {
  const defaultOptions = {
    maxSizeMB: 0.5,
    maxWidthOrHeight: 1920,
    useWebWorker: true,
  };

  try {
    const compressedBlob = await imageCompression(file, {
      ...defaultOptions,
      ...options,
    });

    // Blob を File に変換
    const compressedFile = new File([compressedBlob], file.name, {
      type: file.type,
      lastModified: Date.now(),
    });

    console.log(
      `圧縮完了: ${(file.size / 1024 / 1024).toFixed(2)}MB → ${(compressedFile.size / 1024 / 1024).toFixed(2)}MB`,
    );

    return compressedFile;
  } catch (error) {
    console.error("画像圧縮エラー:", error);
    // 圧縮失敗時は元のファイルを返す
    return file;
  }
}
