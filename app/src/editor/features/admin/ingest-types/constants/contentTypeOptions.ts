import { ContentTypeName, OptionItem } from '@/shared';

export const contentTypeOptions = Object.values(ContentTypeName).map((v) => new OptionItem(v, v));
