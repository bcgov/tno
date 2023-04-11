import { ContentTypeName } from 'tno-core';

export enum HomeFilterType {
  Papers = ContentTypeName.PrintContent,
  RadioTV = ContentTypeName.Snippet,
  Internet = 'internet',
  CPNews = 'cpNews',
}
