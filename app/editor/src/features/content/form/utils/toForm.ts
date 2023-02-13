import { IContentModel } from 'hooks/api-editor';
import moment from 'moment';
import { OptionItem } from 'tno-core';

import { IContentForm } from '../interfaces';

/**
 * Converts the api model into form values.
 * @param model The model from the api.
 * @returns A new instance of form values.
 */
export function toForm(model: IContentModel): IContentForm {
  // return form values in valid API format on submit of ContentForm
  // not utilized properly right now - update coming
  const defaultTonePool = model.tonePools?.find((t) => t.name === 'Default');
  return {
    id: model.id,
    uid: model.uid ?? '',
    sourceUrl: model.sourceUrl ?? '',
    status: model.status,
    contentType: model.contentType,
    summary: model.summary,
    body: model.body ?? '',
    productId: model.productId,
    licenseId: model.licenseId,
    headline: model.headline,
    byline: model.byline ?? '',
    sourceId: model.sourceId,
    otherSource: model.otherSource,
    tempSource: !!model.sourceId ? '' : model.otherSource,
    page: model.page,
    ownerId: model.ownerId ?? '',
    seriesId: model.seriesId,
    otherSeries: '',
    publishedOn: model.publishedOn ?? '',
    publishedOnTime: !!model.publishedOn ? moment(model.publishedOn).format('HH:mm:ss') : '',
    isHidden: model.isHidden,
    isApproved: model.isApproved,
    actions: model.actions ?? [],
    categories: model.categories ?? [],
    tags: model.tags ?? [],
    labels: model.labels ?? [],
    tone: defaultTonePool?.value ?? '',
    tonePool: defaultTonePool
      ? new OptionItem(`${defaultTonePool.value}`, defaultTonePool.value)
      : undefined,
    tonePools: model.tonePools ?? [],
    timeTrackings: model.timeTrackings ?? [],
    fileReferences: model.fileReferences ?? [],
    links: model.links ?? [],
    workOrders: [],
    // Print Content
    section: model.section ?? '',
    edition: model.edition ?? '',
    version: model.version,
  };
}
