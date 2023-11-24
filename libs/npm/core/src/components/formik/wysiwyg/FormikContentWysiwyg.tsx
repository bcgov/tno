import 'react-quill/dist/quill.snow.css';

import { useFormikContext } from 'formik';
import _ from 'lodash';
import React from 'react';

import { IContentModel, ITagModel } from '../../../hooks/api';
import { FormikWysiwyg, IFormikWysiwygProps } from './FormikWysiwyg';

const tagMatch = /\[.*?\]/g;

export interface IFormikContentWysiwyg extends IFormikWysiwygProps<IContentModel> {
  tags?: ITagModel[];
}

/**
 * A Formik WYSIWYG component.
 * @param props Component props.
 * @returns A component.
 */
export const FormikContentWysiwyg = ({ tags, onBlur, ...rest }: IFormikContentWysiwyg) => {
  const { values, setFieldValue } = useFormikContext<IContentModel>();

  const extractTags = React.useCallback(
    (values: string[]) => {
      return (tags ?? [])
        .filter((tag) =>
          values.some((value: string) => value.toLowerCase() === tag.code.toLowerCase()),
        )
        .map((tag) => tag);
    },
    [tags],
  );

  onBlur =
    onBlur ??
    (() => {
      const value = values[rest.name];
      if (!!value && typeof value === 'string' && tags) {
        const stringValue = value.match(tagMatch)?.pop()?.toString().slice(1, -1);
        const tagValues = stringValue?.split(', ') ?? [];
        const tags = extractTags(tagValues);
        if (!_.isEqual(tags, (values as any).tags)) setFieldValue('tags', tags);
      }
    });

  return <FormikWysiwyg onBlur={onBlur} {...rest} />;
};
