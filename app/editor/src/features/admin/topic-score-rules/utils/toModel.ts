import { ITopicScoreRuleModel } from 'hooks/api-editor';

import { ITopicScoreRuleForm } from './../interfaces';

export const toModel = (values: ITopicScoreRuleForm): ITopicScoreRuleModel => {
  return {
    id: values.id,
    sourceId: values.sourceId !== '' ? values.sourceId : 0,
    seriesId: values.seriesId !== '' ? values.seriesId : undefined,
    section: values.section !== '' ? values.section : undefined,
    pageMin: values.pageMin !== '' ? values.pageMin : undefined,
    pageMax: values.pageMax !== '' ? values.pageMax : undefined,
    hasImage: values.hasImage,
    timeMin: values.timeMin !== '' ? values.timeMin : undefined,
    timeMax: values.timeMax !== '' ? values.timeMax : undefined,
    characterMin: values.characterMin !== '' ? values.characterMin : undefined,
    characterMax: values.characterMax !== '' ? values.characterMax : undefined,
    score: values.score !== '' ? values.score : 0,
    sortOrder: values.sortOrder,
    createdBy: values.createdBy,
    createdOn: values.createdOn,
    updatedBy: values.updatedBy,
    updatedOn: values.updatedOn,
    version: values.version,
    remove: values.remove,
  };
};
