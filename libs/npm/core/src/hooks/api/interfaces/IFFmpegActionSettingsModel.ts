import { FFmpegActionName } from '../constants';

export interface IFFmpegActionSettingsModel {
  action: FFmpegActionName;
  arguments: Record<string, string>;
  sortOrder: number;
}
