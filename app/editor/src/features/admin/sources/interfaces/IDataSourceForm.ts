export interface IDataSourceForm {
  id: number;
  name: string;
  code: string;
  description: string;
  enabled: boolean;
  dataLocationId: number | '';
  mediaTypeId: number | '';
  licenseId: number | '';
  topic: string;
  connection: string;
  lastRanOn: Date | '';
  parentId: number | '';
}
