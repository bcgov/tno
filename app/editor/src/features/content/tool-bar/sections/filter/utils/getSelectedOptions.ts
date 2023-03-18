import { showOnlyOptions } from 'features/content/form/constants/showOnlyOptions';
import { IContentListFilter } from 'features/content/list-view/interfaces';
import { ContentTypeName, IOptionItem } from 'tno-core';

// the below function helps sync between the checkboxes and the select dropdown when the screen size is changed for the show only section of the content tool bar
export const getSelectedOptions = (filter: IContentListFilter) => {
  const selectedOptions: IOptionItem[] = [];
  if (filter.contentType === ContentTypeName.PrintContent) selectedOptions.push(showOnlyOptions[0]);
  if (filter.includedInTopic) selectedOptions.push(showOnlyOptions[1]);
  if (filter.onTicker) selectedOptions.push(showOnlyOptions[2]);
  if (filter.commentary) selectedOptions.push(showOnlyOptions[3]);
  if (filter.topStory) selectedOptions.push(showOnlyOptions[4]);
  return selectedOptions;
};
