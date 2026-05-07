import type { SourceUploadIntentResponseDto } from '@/domains/sources/types';

export class SourceUploadError extends Error {
  constructor(message: string, public readonly status?: number) {
    super(message);
    this.name = 'SourceUploadError';
  }
}

export function uploadSourceFile({
  file,
  intentResponse,
  onProgress,
}: {
  file: File;
  intentResponse: SourceUploadIntentResponseDto;
  onProgress?: (percent: number) => void;
}) {
  return new Promise<void>((resolve, reject) => {
    const request = new XMLHttpRequest();
    request.open('PUT', intentResponse.uploadUrl);

    Object.entries(intentResponse.requiredHeaders).forEach(([name, value]) => {
      request.setRequestHeader(name, value);
    });

    request.upload.onprogress = (event) => {
      if (!event.lengthComputable) {
        return;
      }

      onProgress?.(Math.round((event.loaded / event.total) * 100));
    };

    request.onload = () => {
      if (request.status >= 200 && request.status < 300) {
        onProgress?.(100);
        resolve();
        return;
      }

      reject(new SourceUploadError('File upload failed.', request.status));
    };

    request.onerror = () => reject(new SourceUploadError('File upload failed.'));
    request.onabort = () => reject(new SourceUploadError('File upload was canceled.'));
    request.send(file);
  });
}
