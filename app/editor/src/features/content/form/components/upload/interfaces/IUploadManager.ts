export interface IUploadManager {
  upload: (file: File, locationId: number, path: string) => void;
}
