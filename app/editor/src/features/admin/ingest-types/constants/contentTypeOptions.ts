import { ContentTypeName } from 'hooks/api-editor';
import { OptionItem } from 'tno-core';

export const contentTypeOptions = Object.values(ContentTypeName).map((v) => new OptionItem(v, v));
