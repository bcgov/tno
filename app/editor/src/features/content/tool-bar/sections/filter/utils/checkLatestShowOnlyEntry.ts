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
  if (newValues[newValues.length - 1].value === ContentTypeName.PrintContent) {
    onChange({
      ...filter,
      pageIndex: 0,
      contentType: ContentTypeName.PrintContent,
    });
  }
  if ((newValues[newValues.length - 1].label as string).includes('EoD')) {
    onChange({
      ...filter,
      pageIndex: 0,
      includedInCategory: true,
    });
  }
  if ((newValues[newValues.length - 1].label as string).includes('Ticker')) {
    onChange({
      ...filter,
      pageIndex: 0,
      onTicker: 'On Ticker',
    });
  }
  if ((newValues[newValues.length - 1].label as string).includes('Commentary')) {
    onChange({
      ...filter,
      pageIndex: 0,
      commentary: 'Commentary',
    });
  }
  if ((newValues[newValues.length - 1].label as string).includes('Top Story')) {
    onChange({
      ...filter,
      pageIndex: 0,
      topStory: 'Top Story',
    });
  }
};
