import { OptionItem } from 'tno-core';

import { advancedSearchKeys } from '.';

export const advancedSearchOptions = [
  new OptionItem('Headline', advancedSearchKeys.Headline),
  new OptionItem('Source', advancedSearchKeys.Source),
  new OptionItem('Byline', advancedSearchKeys.Byline),
  new OptionItem('Section', advancedSearchKeys.Section),
  new OptionItem('Page', advancedSearchKeys.Page),
  new OptionItem('Status', advancedSearchKeys.Status),
  new OptionItem('Edition', advancedSearchKeys.Edition),
  new OptionItem('Content Type', advancedSearchKeys.ContentType),
  new OptionItem('Published On', advancedSearchKeys.PublishedOn),
  new OptionItem('Created On', advancedSearchKeys.CreatedOn),
  new OptionItem('Updated On', advancedSearchKeys.UpdatedOn),
];
