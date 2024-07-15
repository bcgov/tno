import 'react-quill/dist/quill.snow.css';

import { IContentModel, ITagModel } from '../../../hooks/api';
import { FormikWysiwyg, IFormikWysiwygProps } from './FormikWysiwyg';

export interface IFormikContentWysiwyg extends IFormikWysiwygProps<IContentModel> {
  tags?: ITagModel[];
}

/**
 * A Formik WYSIWYG component.
 * @param props Component props.
 * @returns A component.
 */
export const FormikContentWysiwyg = ({ tags, ...rest }: IFormikContentWysiwyg) => {
  return <FormikWysiwyg {...rest} />;
};
