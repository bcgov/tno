import { IOptionItem } from 'components';

export interface IContentForm {
  id: number;
  summary: string;
  transcription: string;
  mediaType: IOptionItem | undefined;
  headline: string;
  status: number;
}
