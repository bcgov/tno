import { IContentActionModel } from '@/shared';
import { dayjs } from '@/shared/utils/date';

import { isWeekday } from './isWeekday';

export const determineCommentaryTime = (
  commentaryActionId: number,
  actions: IContentActionModel[],
) => {
  const date = new Date();

  const offsetValue =
    actions.find((a) => a.id === commentaryActionId)?.value ?? (isWeekday() ? '3' : '5');
  const offset = +offsetValue;

  date.setHours(date.getHours() - offset);
  return dayjs(date).toISOString();
};
