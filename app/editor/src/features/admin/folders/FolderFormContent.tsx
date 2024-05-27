import { DragDropContentContext } from 'components/content';
import { useFormikContext } from 'formik';
import React from 'react';
import { Col } from 'tno-core';

import { IFolderForm } from './interfaces';

export const FolderFormContent: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IFolderForm>();

  return (
    <Col>
      <DragDropContentContext
        data={values.content}
        onChange={(content) => {
          setFieldValue('content', content);
        }}
        onSelected={(content) => {
          const items = values.content.map((item) =>
            item.content.id === content.content.id ? content : item,
          );
          setFieldValue('content', items);
        }}
        showSortOrder={true}
        showCheckbox={true}
        to={(row) => {
          return `/contents/${row.content.id}`;
        }}
      />
    </Col>
  );
};
