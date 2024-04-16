import { IContentSearchResult } from 'features/utils/interfaces';
import moment from 'moment';

import { determineCommentaryTime } from './determineCommentaryTime';

/**
 * Filters out content that is within the postedOn cutoff time.
 * @param results An array of content.
 * @param commentaryActionId The primary key to the commentary action.
 * @returns An array of filtered content.
 */
export const filterCommentaryResults = (
  results: IContentSearchResult[],
  commentaryActionId: number,
) => {
  return results.filter((content) => {
    const cutoff = determineCommentaryTime(commentaryActionId, content.actions);
    return content.postedOn && moment(content.postedOn) >= moment(cutoff);
  });
};
