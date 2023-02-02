import { ShowOnlyValues } from 'features/content/form/constants/ShowOnlyValues';
import { IContentListFilter } from 'features/content/list-view/interfaces';
import { ContentTypeName } from 'hooks';
import { IOptionItem } from 'tno-core';

/**
 * Function that checks the latest entry in the array of selected options and sets the filter accordingly
 * @param newValues the current array of selected options
 * @param onChange the function to perform on change of the filter
 * @param filter the filter to be updated
 */
export const checkLatestShowOnlyEntry = (
  newValues: IOptionItem[],
  onChange: (filter: IContentListFilter) => void,
  filter: IContentListFilter,
) => {
  // convert to switch statement
  switch (newValues[newValues.length - 1].value) {
    case ShowOnlyValues.PrintContent:
      onChange({
        ...filter,
        pageIndex: 0,
        contentType: ContentTypeName.PrintContent,
      });
      break;
    case ShowOnlyValues.IncludedInEod:
      onChange({
        ...filter,
        pageIndex: 0,
        includedInCategory: true,
      });
      break;
    case ShowOnlyValues.OnTicker:
      onChange({
        ...filter,
        pageIndex: 0,
        onTicker: ShowOnlyValues.OnTicker,
      });
      break;
    case ShowOnlyValues.Commentary:
      onChange({
        ...filter,
        pageIndex: 0,
        commentary: ShowOnlyValues.Commentary,
      });
      break;
    case ShowOnlyValues.TopStory:
      onChange({
        ...filter,
        pageIndex: 0,
        topStory: ShowOnlyValues.TopStory,
      });
      break;
  }
};
