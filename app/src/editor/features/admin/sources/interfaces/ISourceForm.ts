import { type IMediaTypeModel, type ISourceModel } from '@/shared';

export interface ISourceForm extends Omit<
  ISourceModel,
  'mediaTypeId' | 'ownerId' | 'mediaTypeSearchMappings'
> {
  mediaTypeId: number | '';
  mediaTypeSearchMappings: IMediaTypeModel[];
  ownerId: number | '';
}
