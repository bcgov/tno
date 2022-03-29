import { IOptionItem, OptionItem } from 'components/form';
import { IDataSourceModel } from 'hooks/api-editor';

export const sortDataSource = <T extends IDataSourceModel>(a: T, b: T) => {
  if (a.name < b.name) return -1;
  if (a.name > b.name) return 1;
  if (a.code < b.code) return -1;
  if (a.code > b.code) return 1;
  return 0;
};

const displayName = (dataSource: IDataSourceModel) => {
  return dataSource.code;
};

export const getDataSourceOptions = <T extends IDataSourceModel>(
  items: T[],
  prepend: IOptionItem[] = [],
  map: (item: T) => IOptionItem = (ds) => new OptionItem(displayName(ds), ds.id),
) => {
  return prepend.concat([...items].sort(sortDataSource).map(map));
};
