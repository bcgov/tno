import { ITagModel, OptionItem } from 'tno-core';

export const getTagOptions = (tags: ITagModel[]) => {
  return tags.map((t) => new OptionItem(`${t.code} - ${t.name}`, t.code, !t.isEnabled));
};
