import { ITopicScoreRuleModel } from 'hooks/api-editor';

import { IContentForm } from '../interfaces';

/**
 * Calculate the score for the topic.
 * @param values The content form values.
 * @param rules An array of topic score rules.
 * @returns The score or an empty string.
 */
export const calcTopicScore = (values: IContentForm, rules: ITopicScoreRuleModel[]) => {
  var score: string | number = '';

  rules
    .filter((r) => r.sourceId === values.sourceId)
    .every((r) => {
      // Eliminate any rule that doesn't match the content values.
      if (r.section !== undefined && values.section !== r.section) return true;

      if (r.seriesId !== undefined && values.seriesId !== r.seriesId) return true;

      if (
        r.pageMin !== undefined &&
        r.pageMax !== undefined &&
        (+values.page === 0 || +values.page < r.pageMin || +values.page > r.pageMax)
      )
        return true;
      else if (r.pageMin !== undefined && (+values.page === 0 || +values.page < r.pageMin))
        return true;
      else if (r.pageMax !== undefined && (+values.page === 0 || +values.page > r.pageMax))
        return true;

      // if (r.hasImage !== undefined && values.hasImage) return true; // TODO: We need a way to identify a story has an image.

      if (
        r.characterMin !== undefined &&
        r.characterMax !== undefined &&
        (values.body.length < r.characterMin || values.body.length > r.characterMax)
      )
        return true;
      else if (r.characterMin !== undefined && values.body.length < r.characterMin) return true;
      else if (r.characterMax !== undefined && values.body.length > r.characterMax) return true;

      if (
        r.timeMin !== undefined &&
        r.timeMax !== undefined &&
        (values.publishedOnTime < r.timeMin || values.publishedOnTime > r.timeMax)
      )
        return true;
      else if (r.timeMin !== undefined && values.publishedOnTime < r.timeMin) return true;
      else if (r.timeMax !== undefined && values.publishedOnTime > r.timeMax) return true;

      // Take the first matching rule.
      score = r.score;
      return false;
    });

  return score;
};
