import { IMediaTypeModel, ISourceModel } from 'tno-core';

export interface ISourceForm
  extends Omit<ISourceModel, 'mediaTypeId' | 'ownerId' | 'mediaTypeSearchMappings'> {
  mediaTypeId: number | '';
  mediaTypeSearchMappings: IMediaTypeModel[];
  ownerId: number | '';
}
