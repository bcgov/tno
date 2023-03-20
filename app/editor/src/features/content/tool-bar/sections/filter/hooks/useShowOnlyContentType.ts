import { ShowOnlyValues } from 'features/content/form/constants/ShowOnlyValues';
import { IContentListFilter } from 'features/content/list-view/interfaces';
import React from 'react';
import { ActionDelegate } from 'store';
import { useContent } from 'store/hooks';
import { ContentTypeName, IOptionItem, replaceQueryParams } from 'tno-core';

/**
 * Function that checks the latest entry in the array of selected options and sets the filter accordingly
 */
export const useShowOnlyContentType = () => {
  const [{ filterAdvanced }, { storeFilter }] = useContent();

  const onChange = React.useCallback(
    (action: ActionDelegate<IContentListFilter>) => {
      storeFilter((filter) => {
        var result = typeof action === 'function' ? action(filter) : action;
        replaceQueryParams({ ...result, ...filterAdvanced }, { includeEmpty: false });
        return result;
      });
    },
    [filterAdvanced, storeFilter],
  );

  return (newValues: IOptionItem[]) => {
    // convert to switch statement
    switch (newValues[newValues.length - 1].value) {
      case ShowOnlyValues.PrintContent:
        onChange((filter) => {
          return {
            ...filter,
            pageIndex: 0,
            contentType: ContentTypeName.PrintContent,
          };
        });

        break;
      case ShowOnlyValues.IncludedInEod:
        onChange((filter) => ({
          ...filter,
          pageIndex: 0,
          includedInTopic: true,
        }));
        break;
      case ShowOnlyValues.Commentary:
        onChange((filter) => ({
          ...filter,
          pageIndex: 0,
          commentary: ShowOnlyValues.Commentary,
        }));
        break;
      case ShowOnlyValues.TopStory:
        onChange((filter) => ({
          ...filter,
          pageIndex: 0,
          topStory: ShowOnlyValues.TopStory,
        }));
        break;
    }
  };
};
