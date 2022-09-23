import { OptionItem } from 'components/form';
import { ContentTypeName } from 'hooks/api-editor';

export const contentTypeOptions = Object.values(ContentTypeName).map((v) => new OptionItem(v, v));
