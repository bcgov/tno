import {
  IActionModel,
  IContributorModel,
  IFilterModel,
  IProductModel,
  ISeriesModel,
  ISourceModel,
} from 'tno-core';

export const parseExportedFilter = (
  value: any,
  actions: IActionModel[],
  contributors: IContributorModel[],
  series: ISeriesModel[],
  sources: ISourceModel[],
  products: IProductModel[],
): IFilterModel => {
  var importedModel = {};
  if ('name' in value) {
    importedModel = { ...importedModel, name: value.name };
  }
  if ('description' in value) {
    importedModel = { ...importedModel, description: value.description };
  }
  if ('isEnabled' in value) {
    importedModel = { ...importedModel, isEnabled: value.isEnabled };
  }
  var importedSettings = {};
  if ('size' in value.settings) {
    importedSettings = { ...importedSettings, size: value.settings.size ?? 10 };
  }
  if ('search' in value.settings) {
    importedSettings = { ...importedSettings, search: value.settings.search };
  }
  if ('inHeadline' in value.settings) {
    importedSettings = { ...importedSettings, inHeadline: value.settings.inHeadline };
  }
  if ('inStory' in value.settings) {
    importedSettings = { ...importedSettings, inStory: value.settings.inStory };
  }
  if ('inByline' in value.settings) {
    importedSettings = { ...importedSettings, inByline: value.settings.inByline };
  }
  if ('edition' in value.settings) {
    importedSettings = { ...importedSettings, edition: value.settings.edition };
  }
  if ('section' in value.settings) {
    importedSettings = { ...importedSettings, section: value.settings.section };
  }
  if ('page' in value.settings) {
    importedSettings = { ...importedSettings, page: value.settings.page };
  }
  if ('contentTypes' in value.settings) {
    importedSettings = { ...importedSettings, contentTypes: value.settings.contentTypes };
  }
  if ('tags' in value.settings) {
    importedSettings = { ...importedSettings, tags: value.settings.tags };
  }
  if ('sentiment' in value.settings) {
    importedSettings = { ...importedSettings, sentiment: value.settings.sentiment };
  }
  if ('dateOffset' in value.settings) {
    importedSettings = { ...importedSettings, dateOffset: value.settings.dateOffset };
  } else {
    if ('startDate' in value.settings) {
      importedSettings = { ...importedSettings, startDate: value.settings.startDate };
    }
    if ('endDate' in value) {
      importedSettings = { ...importedSettings, endDate: value.settings.endDate };
    }
  }

  // check the import JSON for values which may need to be mapped
  if ('actionNames' in value.settings) {
    const importedActions = actions
      .filter((x) => value.settings.actionNames.some((y: string) => y === x.name))
      .map((x) => ({ id: x.id, value: true, valueType: x.valueType }));
    importedSettings = { ...importedSettings, actions: importedActions };
  }
  if ('seriesNames' in value.settings) {
    const seriesIds = series
      .filter((x) => value.settings.seriesNames.some((y: string) => y === x.name))
      .map((x) => x.id);
    importedSettings = { ...importedSettings, seriesIds: seriesIds };
  }
  if ('sourceCodes' in value.settings) {
    const sourceIds = sources
      .filter((x) => value.settings.sourceCodes.some((y: string) => y === x.code))
      .map((x) => x.id);
    importedSettings = { ...importedSettings, sourceIds: sourceIds };
  }
  if ('productNames' in value.settings) {
    const productIds = products
      .filter((x) => value.settings.productNames.some((y: string) => y === x.name))
      .map((x) => x.id);
    importedSettings = { ...importedSettings, productIds: productIds };
  }
  if ('contributorNames' in value.settings) {
    const contributorIds = contributors
      .filter((x) => value.settings.contributorNames.some((y: string) => y === x.name))
      .map((x) => x.id);
    importedSettings = { ...importedSettings, contributorIds: contributorIds };
  }
  importedModel = { ...importedModel, settings: importedSettings };

  return importedModel as IFilterModel;
};
