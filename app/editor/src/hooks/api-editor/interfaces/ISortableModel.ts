export interface ISortableModel<T extends string | number> {
  id: T;
  name: string;
  sortOrder: number;
}
