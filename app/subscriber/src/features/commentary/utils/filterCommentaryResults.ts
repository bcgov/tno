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
    const postedCutoff = determineCommentaryTime(commentaryActionId, content.actions);
    const publishedCutoff = moment().subtract(1, 'month');
    let commentaryUpdatedOn = null;
    // use the commentary action updated on time to decide if the content should be shown in the commentary list
    const commentaryAction = content.actions.find((a) => a.id === commentaryActionId);
    if (commentaryAction) {
      commentaryUpdatedOn = commentaryAction.updatedOn;
    }
    return (
      commentaryUpdatedOn &&
      moment(commentaryUpdatedOn) >= moment(postedCutoff) &&
      moment(content.publishedOn) >= publishedCutoff
    );
  });
};
