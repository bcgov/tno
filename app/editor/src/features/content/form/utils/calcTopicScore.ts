import { ITopicScoreRuleModel } from 'tno-core';

import { IContentForm } from '../interfaces';

/**
 * Calculate the score for the topic.
 * @param rules An array of topic score rules.
 * @param values The content form values.
 * @returns The score or an empty string.
 */
export const calcTopicScoreForContentForm = (
  rules: ITopicScoreRuleModel[],
  values: IContentForm,
) => {
  return calcTopicScore(
    rules,
    values.sourceId,
    values.body.length,
    values.publishedOnTime,
    values.page,
    values.section,
    values.seriesId,
  );
};

/**
 * Calculate the score for the topic.
 * @param rules An array of topic score rules.
 * @param sourceId Id of Source
 * @param bodyLength Length opf the content body
 * @param publishedOnTime Time of day the content was published
 * @param page page "number" for print content
 * @param section section for print content
 * @param seriesId Id of Series for A/V content
 * @returns The score or an empty string.
 */
export const calcTopicScore = (
  rules: ITopicScoreRuleModel[],
  sourceId: number | undefined,
  bodyLength: number,
  publishedOnTime: string,
  page: string,
  section: string | undefined,
  seriesId: number | undefined,
) => {
  var score: number = 0;

  rules
    .filter((r) => r.sourceId === sourceId)
    .every((r) => {
      // Eliminate any rule that doesn't match the content values by returning true.

      // rule section doesnt match the section on the content
      if (r.section !== undefined && section !== r.section) return true;

      // rule series doesnt match the series on the content
      if (r.seriesId !== undefined && seriesId !== r.seriesId) return true;

      const riPageMin = r.pageMin !== undefined ? r.pageMin.search(/\d*$/) : undefined;
      const riPageMax = r.pageMax !== undefined ? r.pageMax.search(/\d*$/) : undefined;
      const rPageMin = riPageMin !== undefined ? Number(r.pageMin?.slice(riPageMin)) : undefined;
      const rPageMax = riPageMax !== undefined ? Number(r.pageMax?.slice(riPageMax)) : undefined;
      const cPageMin = riPageMin !== undefined ? r.pageMin?.slice(0, riPageMin) : r.pageMin;
      const cPageMax = riPageMax !== undefined ? r.pageMax?.slice(0, riPageMax) : r.pageMax;

      const viPage = page !== '' ? page.search(/\d*$/) : undefined;
      const vPage = viPage !== undefined ? Number(page?.slice(viPage)) : 0;
      const vcPage = viPage !== undefined ? page?.slice(0, viPage) : page;

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

      // if (r.hasImage !== undefined && hasImage) return true; // TODO: We need a way to identify a story has an image.

      if (
        r.characterMin !== undefined &&
        r.characterMax !== undefined &&
        (bodyLength < r.characterMin || bodyLength > r.characterMax)
      ) {
        return true;
      } else if (r.characterMin !== undefined && bodyLength < r.characterMin) return true;
      else if (r.characterMax !== undefined && bodyLength > r.characterMax) return true;

      if (
        r.timeMin !== undefined &&
        r.timeMax !== undefined &&
        (publishedOnTime < r.timeMin || publishedOnTime > r.timeMax)
      ) {
        return true;
      } else if (r.timeMin !== undefined && publishedOnTime < r.timeMin) return true;
      else if (r.timeMax !== undefined && publishedOnTime > r.timeMax) return true;

      // Take the first matching rule.
      score = r.score;
      return false;
    });

  return score;
};
