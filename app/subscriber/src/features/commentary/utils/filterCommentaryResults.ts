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
    let commentaryCreatedOn = null;
    // use the commentary action created on time to decide if the content should be shown in the commentary list
    const commentaryAction = content.actions.find((a) => a.id === commentaryActionId);
    if (commentaryAction) {
      commentaryCreatedOn = commentaryAction.createdOn;
    }
    return (
      commentaryCreatedOn &&
      moment(commentaryCreatedOn) >= moment(postedCutoff) &&
      moment(content.publishedOn) >= publishedCutoff
    );
  });
};
