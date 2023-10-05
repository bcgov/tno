import { ContentTypeName, OptionItem } from 'tno-core';

export const contentTypeOptions = Object.values(ContentTypeName).map((v) => new OptionItem(v, v));
