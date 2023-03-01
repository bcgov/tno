import { IContentForm } from 'features/content/form/interfaces';
import { useFormikContext } from 'formik';
import _ from 'lodash';
import React from 'react';
import { Button, ButtonVariant, FieldSize, FormikText } from 'tno-core';

import * as styled from './styled';

export interface ITagsProps {
  fieldName: string;
}

/**
 * The component that renders tags for a given text field
 * @returns the Tags component
 */
export const Tags: React.FC<ITagsProps> = ({ fieldName }) => {
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const tagMatch = /\[.*?\]/g;

  // Ensure tag order does not change
  React.useEffect(() => {
    const sortedTags = _.orderBy(values.tags, [(tag) => tag.code.toLowerCase()], ['asc']);
    if (!_.isEqual(sortedTags, values.tags)) {
      setFieldValue('tags', sortedTags);
    }
  }, [setFieldValue, values.tags]);
  return (
    <styled.Tags className="multi-group">
      <FormikText
        name="tags"
        label="Tags"
        disabled
        width={FieldSize.Medium}
        value={values.tags.map((t) => t.code).join(', ')}
      />
      <Button
        variant={ButtonVariant.danger}
        className="clear-tags"
        onClick={() => {
          setFieldValue(fieldName, values.summary.replace(tagMatch, ''));
          setFieldValue('tags', []);
        }}
      >
        X
      </Button>
    </styled.Tags>
  );
};
