import { OptionItem } from 'tno-core';

import { AdvancedSearchKeys } from '.';

export const advancedSearchOptions = [
  new OptionItem('Id', AdvancedSearchKeys.Id),
  new OptionItem('Headline', AdvancedSearchKeys.Headline),
  new OptionItem('Summary', AdvancedSearchKeys.Summary),
  new OptionItem('Story', AdvancedSearchKeys.Story),
  new OptionItem('Transcript', AdvancedSearchKeys.Body),
  new OptionItem('Source', AdvancedSearchKeys.Source),
  new OptionItem('Byline', AdvancedSearchKeys.Byline),
  new OptionItem('Section', AdvancedSearchKeys.Section),
  new OptionItem('Page', AdvancedSearchKeys.Page),
  new OptionItem('Status', AdvancedSearchKeys.Status),
  new OptionItem('Edition', AdvancedSearchKeys.Edition),
  new OptionItem('Content Type', AdvancedSearchKeys.ContentType),
  new OptionItem('Published On', AdvancedSearchKeys.PublishedOn),
  new OptionItem('Created On', AdvancedSearchKeys.CreatedOn),
  new OptionItem('Updated On', AdvancedSearchKeys.UpdatedOn),
];
