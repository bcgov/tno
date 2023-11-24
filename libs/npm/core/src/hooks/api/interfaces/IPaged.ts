export interface IPaged<IT> {
  page: number;
  quantity: number;
  total: number;
  items: IT[];
}
