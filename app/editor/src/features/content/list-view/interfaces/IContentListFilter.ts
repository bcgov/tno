import { LogicalOperator } from 'hooks';

export interface IContentListFilter {
  mediaTypeId: number | '';
  ownerId: number | '';
  newspaper: boolean;
  included: boolean;
  onTicker: boolean;
  commentary: boolean;
  topStory: boolean;
  fieldType: string;
  logicalOperator: LogicalOperator | '';
  searchTerm: string;
  createdStartOn?: Date | null;
  createdEndOn?: Date | null;
}
