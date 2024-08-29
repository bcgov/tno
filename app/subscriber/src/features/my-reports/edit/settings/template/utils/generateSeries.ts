import { ISeriesModel } from 'tno-core';

interface IGenerateSeriesOptions {
  _id?: number;
  init?: Partial<ISeriesModel>;
}

export const generateSeries = (options: IGenerateSeriesOptions) => {
  const entity: ISeriesModel = {
    id: options._id ?? 0,
    name: `Program ${options?._id ?? 0}`,
    description: '',
    sortOrder: 0,
    isEnabled: true,
    isOther: false,
    autoTranscribe: false,
    useInTopics: false,
    ...options.init,
  };
  return entity;
};
