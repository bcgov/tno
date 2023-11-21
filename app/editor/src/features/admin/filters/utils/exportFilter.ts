import {
  IActionModel,
  IContributorModel,
  IMediaTypeModel,
  ISeriesModel,
  ISourceModel,
} from 'tno-core';

import { FilterImportExportModel } from './FilterImportExportModel';
import { IFilterImportExportModel } from './IFilterImportExportModel';

export const exportFilter = (
  name: string,
  description: string,
  currentSettings: any,
  actions: IActionModel[],
  contributors: IContributorModel[],
  series: ISeriesModel[],
  sources: ISourceModel[],
  mediaTypes: IMediaTypeModel[],
): IFilterImportExportModel => {
  var exportFilter = new FilterImportExportModel();
  exportFilter.name = name;
  exportFilter.description = description;
  var exportSettings = {};
  exportSettings = { ...exportSettings, size: currentSettings.size ?? 10 };
  if ('search' in currentSettings) {
    exportSettings = { ...exportSettings, search: currentSettings.search };
  }
  if ('inHeadline' in currentSettings) {
    exportSettings = { ...exportSettings, inHeadline: currentSettings.inHeadline };
  }
  if ('inStory' in currentSettings) {
    exportSettings = { ...exportSettings, inStory: currentSettings.inStory };
  }
  if ('inByline' in currentSettings) {
    exportSettings = { ...exportSettings, inByline: currentSettings.inByline };
  }
  if ('edition' in currentSettings) {
    exportSettings = { ...exportSettings, edition: currentSettings.edition };
  }
  if ('section' in currentSettings) {
    exportSettings = { ...exportSettings, section: currentSettings.section };
  }
  if ('page' in currentSettings) {
    exportSettings = { ...exportSettings, page: currentSettings.page };
  }
  if ('contentTypes' in currentSettings) {
    exportSettings = { ...exportSettings, contentTypes: currentSettings.contentTypes };
  }
  if ('tags' in currentSettings) {
    exportSettings = { ...exportSettings, tags: currentSettings.tags };
  }
  if ('sentiment' in currentSettings) {
    exportSettings = { ...exportSettings, sentiment: currentSettings.sentiment };
  }
  if ('dateOffset' in currentSettings) {
    exportSettings = { ...exportSettings, dateOffset: currentSettings.dateOffset };
  } else {
    if ('startDate' in currentSettings) {
      exportSettings = { ...exportSettings, startDate: currentSettings.startDate };
    }
    if ('endDate' in currentSettings) {
      exportSettings = { ...exportSettings, endDate: currentSettings.endDate };
    }
  }

  // check the import JSON for currentSettingss which may need to be mapped
  if ('actions' in currentSettings) {
    const actionNames = actions
      .filter((x) => currentSettings.actions.some((y: IActionModel) => y.id === x.id))
      .map((x) => x.name);
    exportSettings = { ...exportSettings, actionNames: actionNames };
  }
  if ('seriesIds' in currentSettings) {
    const seriesNames = series
      .filter((x) => currentSettings.seriesIds.some((y: number) => y === x.id))
      .map((x) => x.name);
    exportSettings = { ...exportSettings, seriesNames: seriesNames };
  }
  if ('sourceIds' in currentSettings) {
    const sourceCodes = sources
      .filter((x) => currentSettings.sourceIds.some((y: number) => y === x.id))
      .map((x) => x.code);
    exportSettings = { ...exportSettings, sourceCodes: sourceCodes };
  }
  if ('mediaTypeIds' in currentSettings) {
    const mediaTypeNames = mediaTypes
      .filter((x) => currentSettings.mediaTypeIds.some((y: number) => y === x.id))
      .map((x) => x.name);
    exportSettings = { ...exportSettings, mediaTypeNames: mediaTypeNames };
  }
  if ('contributorIds' in currentSettings) {
    const contributorNames = contributors
      .filter((x) => currentSettings.contributorIds.some((y: number) => y === x.id))
      .map((x) => x.name);
    exportSettings = { ...exportSettings, contributorNames: contributorNames };
  }
  exportFilter.settings = exportSettings;

  return exportFilter;
};
