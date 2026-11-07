import { IReportInstanceContentModel } from 'tno-core';

import { IReportForm } from '../interfaces';

/**
 * Whether saving the report must also save the current instance's stories.
 * Stories are saved only when they are loaded and were changed, so a settings change never sends
 * (or overwrites) tens of thousands of stories, and an instance whose stories were never loaded
 * can never be saved as empty.
 * @param values The form being saved.
 * @param sanitized The same form after sanitizing, which may remove duplicate titles.
 * @param baseline The stories as last loaded from the API.
 * @param editable Whether the current instance may still be changed.
 */
export const shouldSaveContent = (
  values: IReportForm,
  sanitized: IReportForm,
  baseline: IReportInstanceContentModel[] | undefined,
  editable: boolean,
) => {
  const instance = values.instances.length ? values.instances[0] : undefined;
  if (!editable || !instance || !values.contentLoaded) return false;
  // Every change to the stories replaces the array.
  if (instance.content !== baseline) return true;
  // Sanitizing removed duplicate titles that must be saved.
  return (sanitized.instances[0]?.content.length ?? 0) !== instance.content.length;
};
