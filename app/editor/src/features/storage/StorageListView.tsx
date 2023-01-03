import { FormikForm } from 'components/formik';
import { ContentClipForm } from 'features/content';
import { defaultFormValues } from 'features/content/form/constants';
import { IContentForm } from 'features/content/form/interfaces';
import { ContentTypeName, useCombinedView, useTooltips } from 'hooks';
import React from 'react';

import * as styled from './styled';

export const StorageListView: React.FC = (props) => {
  const [, setClipErrors] = React.useState<string>('');
  const { formType } = useCombinedView(ContentTypeName.Snippet);
  const [contentType] = React.useState(formType ?? ContentTypeName.Snippet);
  const [form, setForm] = React.useState<IContentForm>({
    ...defaultFormValues(contentType),
  });

  useTooltips();

  return (
    <styled.StorageListView>
      <FormikForm onSubmit={() => {}} initialValues={form}>
        <ContentClipForm content={form} setContent={setForm} setClipErrors={setClipErrors} />
      </FormikForm>
    </styled.StorageListView>
  );
};
