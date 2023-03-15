import { ITopicScoreRuleModel } from 'hooks/api-editor';

import { ITopicScoreRuleForm } from './../interfaces';

export const toForm = (model: ITopicScoreRuleModel): ITopicScoreRuleForm => {
  return {
    id: model.id,
    sourceId: model.sourceId !== undefined ? model.sourceId : '',
    seriesId: model.seriesId !== undefined ? model.seriesId : '',
    section: model.section !== undefined ? model.section : '',
    pageMin: model.pageMin !== undefined ? model.pageMin : '',
    pageMax: model.pageMax !== undefined ? model.pageMax : '',
    hasImage: model.hasImage,
    timeMin: model.timeMin !== undefined ? model.timeMin : '',
    timeMax: model.timeMax !== undefined ? model.timeMax : '',
    characterMin: model.characterMin !== undefined ? model.characterMin : '',
    characterMax: model.characterMax !== undefined ? model.characterMax : '',
    score: model.score !== undefined ? model.score : '',
    sortOrder: model.sortOrder,
    createdBy: model.createdBy,
    createdOn: model.createdOn,
    updatedBy: model.updatedBy,
    updatedOn: model.updatedOn,
    version: model.version,
  };
};
