import { ITopicScoreRuleModel } from 'tno-core';

import { IContentForm } from '../interfaces';

/**
 * Calculate the score for the topic.
 * @param values The content form values.
 * @param rules An array of topic score rules.
 * @returns The score or an empty string.
 */
export const calcTopicScore = (values: IContentForm, rules: ITopicScoreRuleModel[]) => {
  var score: number = 0;

  rules
    .filter((r) => r.sourceId === values.sourceId)
    .every((r) => {
      // Eliminate any rule that doesn't match the content values by returning true.

      // rule section doesnt match the section on the content
      if (r.section !== undefined && values.section !== r.section) return true;

      // rule series doesnt match the series on the content
      if (r.seriesId !== undefined && values.seriesId !== r.seriesId) return true;

      const riPageMin = r.pageMin !== undefined ? r.pageMin.search(/\d*$/) : undefined;
      const riPageMax = r.pageMax !== undefined ? r.pageMax.search(/\d*$/) : undefined;
      const rPageMin = riPageMin !== undefined ? Number(r.pageMin?.slice(riPageMin)) : undefined;
      const rPageMax = riPageMax !== undefined ? Number(r.pageMax?.slice(riPageMax)) : undefined;
      const cPageMin = riPageMin !== undefined ? r.pageMin?.slice(0, riPageMin) : r.pageMin;
      const cPageMax = riPageMax !== undefined ? r.pageMax?.slice(0, riPageMax) : r.pageMax;

      const viPage = values.page !== '' ? values.page.search(/\d*$/) : undefined;
      const vPage = viPage !== undefined ? Number(values.page?.slice(viPage)) : 0;
      const vcPage = viPage !== undefined ? values.page?.slice(0, viPage) : values.page;

      if (
        (rPageMin !== undefined &&
          rPageMax !== undefined &&
          (vPage === 0 || vPage < rPageMin || vPage > rPageMax)) ||
        (cPageMin !== undefined &&
          vcPage !== cPageMin &&
          cPageMax !== undefined &&
          vcPage !== cPageMax)
      ) {
        return true;
      } else if (
        (rPageMin !== undefined && (vPage === 0 || vPage < rPageMin)) ||
        (cPageMin !== undefined &&
          vcPage !== cPageMin &&
          cPageMax !== undefined &&
          vcPage !== cPageMax)
      ) {
        return true;
      } else if (
        (rPageMax !== undefined && (vPage === 0 || vPage > rPageMax)) ||
        (cPageMin !== undefined &&
          vcPage !== cPageMin &&
          cPageMax !== undefined &&
          vcPage !== cPageMax)
      ) {
        return true;
      }

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
