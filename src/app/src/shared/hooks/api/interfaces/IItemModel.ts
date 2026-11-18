export interface IItemModel {
  name: string;
  extension?: string;
  isDirectory: boolean;
  size?: number;
  mimeType?: string;
  modified?: string;
  isLocal: boolean;
}
