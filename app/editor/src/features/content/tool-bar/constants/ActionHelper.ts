import { ContentTypeName, IActionModel } from 'hooks';

export type ActionHelper = {
  init?: boolean;
  determineActions: () => (a: IActionModel) => boolean;
  contentType: ContentTypeName;
};
