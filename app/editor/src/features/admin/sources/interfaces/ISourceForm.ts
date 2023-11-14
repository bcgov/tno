import { ISourceModel } from 'tno-core';

export interface ISourceForm
  extends Omit<ISourceModel, 'mediaTypeId' | 'ownerId' | 'mediaTypeSearchGroupId'> {
  mediaTypeId: number | '';
  mediaTypeSearchGroupId: number | '';
  ownerId: number | '';
}
