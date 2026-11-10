import { KnnSearchResponse } from '@elastic/elasticsearch/lib/api/types';
import { IContentModel, IFilterSettingsModel, IOptionItem } from 'tno-core';

export interface IContentState {
  // each page has own content and filter state
  frontPage: {
    content?: KnnSearchResponse<IContentModel>;
    filter: IFilterSettingsModel;
  };
  home: {
    content?: KnnSearchResponse<IContentModel>;
    filter: IFilterSettingsModel;
  };
  mediaType: {
    content?: KnnSearchResponse<IContentModel>;
    filter: IFilterSettingsModel;
  };

  myMinister: {
    content?: KnnSearchResponse<IContentModel>;
    filter: IFilterSettingsModel;
  };
  pressGalleryFilter: {
    dateFilter?: IOptionItem | null;
    pressFilter?: IOptionItem | null;
    filter: IFilterSettingsModel;
  };
  search: {
    content?: KnnSearchResponse<IContentModel>;
    filter: IFilterSettingsModel;
  };
  todaysCommentary: {
    content?: KnnSearchResponse<IContentModel>;
    filter: IFilterSettingsModel;
  };
  topStories: {
    content?: KnnSearchResponse<IContentModel>;
    filter: IFilterSettingsModel;
  };
  avOverview: {
    filter: IFilterSettingsModel;
  };
  eventOfTheDay: {
    filter: IFilterSettingsModel;
  };
  searchResults: {
    filter: IFilterSettingsModel;
  };
}
