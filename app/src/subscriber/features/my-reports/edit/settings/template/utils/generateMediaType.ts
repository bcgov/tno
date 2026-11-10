import { IMediaTypeModel, ListOptionName } from 'tno-core';

interface IGenerateMediaTypeOptions {
  _id?: number;
  init?: Partial<IMediaTypeModel>;
}

export const generateMediaType = (options: IGenerateMediaTypeOptions) => {
  const entity: IMediaTypeModel = {
    id: options._id ?? 0,
    name: `Media Type ${options?._id ?? 0}`,
    description: '',
    sortOrder: 0,
    isEnabled: true,
    autoTranscribe: false,
    listOption: ListOptionName.Source,
    settings: {},
    ...options.init,
  };
  return entity;
};
