import { OptionItem } from 'components/form';
import { IContentModel } from 'hooks/api-editor';

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
    sourceId: model.sourceId,
    otherSource: model.otherSource,
    tempSource: !!model.sourceId ? '' : model.otherSource,
    page: model.page,
    ownerId: model.ownerId ?? '',
    seriesId: model.seriesId,
    otherSeries: '',
    publishedOn: model.publishedOn?.toString() ?? '',
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
    // Print Content
    section: model.printContent?.section ?? '',
    edition: model.printContent?.edition ?? '',
    storyType: model.printContent?.storyType ?? '',
    byline: model.printContent?.byline ?? '',
    version: model.version,
  };
}
