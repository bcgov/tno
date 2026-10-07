import { type ITopicScoreRuleModel } from 'tno-core';

import { type ITopicScoreRuleForm } from '../interfaces';
import { parsePage } from './page';

export const toForm = (model: ITopicScoreRuleModel): ITopicScoreRuleForm => {
  const pageMin = parsePage(model.pageMin);
  const pageMax = parsePage(model.pageMax);
  return {
    id: model.id,
    sourceId: model.sourceId,
    seriesId: model.seriesId ?? '',
    section: model.section ?? '',
    pagePrefix: pageMin?.prefix ?? pageMax?.prefix ?? '',
    pageMin: pageMin?.number ?? '',
    pageMax: pageMax?.number ?? '',
    hasImage: model.hasImage === undefined || model.hasImage === null ? '' : `${model.hasImage}`,
    timeMin: model.timeMin ?? '',
    timeMax: model.timeMax ?? '',
    characterMin: model.characterMin ?? '',
    characterMax: model.characterMax ?? '',
    score: model.score,
    sortOrder: model.sortOrder,
    createdBy: model.createdBy,
    createdOn: model.createdOn,
    updatedBy: model.updatedBy,
    updatedOn: model.updatedOn,
    version: model.version,
  };
};
