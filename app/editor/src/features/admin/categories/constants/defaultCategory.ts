import { CategoryTypeName, ICategoryModel } from 'hooks';

export const defaultCategory: ICategoryModel = {
  id: 0,
  name: '',
  description: '',
  isEnabled: true,
  sortOrder: 0,
  categoryType: CategoryTypeName.Issues,
  autoTranscribe: false,
};
