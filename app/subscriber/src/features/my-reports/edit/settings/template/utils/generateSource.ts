import { ISourceModel } from 'tno-core';

interface IGenerateSourceOptions {
  _id?: number;
  init?: Partial<ISourceModel>;
}

export const generateSource = (options: IGenerateSourceOptions) => {
  const entity: ISourceModel = {
    id: options?._id ?? 0,
    code: `SOURCE_${options?._id ?? 0}`,
    name: `Source ${options?._id ?? 0}`,
    shortName: `Source ${options?._id ?? 0}`,
    description: '',
    sortOrder: 0,
    isEnabled: true,
    licenseId: 0,
    license: undefined,
    ownerId: 0,
    mediaTypeId: 0,
    mediaType: undefined,
    mediaTypeSearchMappings: [],
    owner: undefined,
    autoTranscribe: false,
    disableTranscribe: false,
    useInTopics: false,
    configuration: {},
    actions: [],
    metrics: [],
    ...options.init,
  };
  return entity;
};
