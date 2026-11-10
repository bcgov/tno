import { type ITopicScoreRuleModel } from 'tno-core';

import { type ITopicScoreRuleForm } from '../interfaces';

const toNumber = (value: number | '') => (value === '' ? undefined : Number(value));
const toText = (value: string) => (value.trim() === '' ? undefined : value.trim());

export const toModel = (values: ITopicScoreRuleForm): ITopicScoreRuleModel => {
  const prefix = values.pagePrefix.trim().toUpperCase();
  return {
    id: values.id,
    sourceId: values.sourceId,
    seriesId: toNumber(values.seriesId),
    section: toText(values.section),
    pageMin: values.pageMin === '' ? undefined : `${prefix}${values.pageMin}`,
    pageMax: values.pageMax === '' ? undefined : `${prefix}${values.pageMax}`,
    hasImage: values.hasImage === '' ? undefined : values.hasImage === 'true',
    timeMin: toText(values.timeMin),
    timeMax: toText(values.timeMax),
    characterMin: toNumber(values.characterMin),
    characterMax: toNumber(values.characterMax),
    score: values.score === '' ? 0 : Number(values.score),
    sortOrder: values.sortOrder,
    createdBy: values.createdBy,
    createdOn: values.createdOn,
    updatedBy: values.updatedBy,
    updatedOn: values.updatedOn,
    version: values.version,
  };
};
