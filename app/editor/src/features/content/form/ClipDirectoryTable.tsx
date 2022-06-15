import { useFormikContext } from 'formik';
import { IItemModel } from 'hooks/api-editor';
import React from 'react';
import { GridTable } from 'tno-core';

import { ClipDirectoryFilter } from './ClipDirectoryFilter';
import { clipDirectoryColumns } from './constants';
import * as styled from './styled';

export interface IClipDirectoryTableProps {
  /** the data to be displayed in the table */
  data: IItemModel[];
  deleteItem: (item: IItemModel) => void;
  selectItem: (item: IItemModel) => void;
  downloadItem: (item: IItemModel) => void;
  attachItem: (item: IItemModel) => void;
  navigate: (item: IItemModel) => void;
}

/** Table used to display time log for users creating and updating content. */
export const ClipDirectoryTable: React.FC<IClipDirectoryTableProps> = ({
  data,
  deleteItem,
  selectItem,
  downloadItem,
  attachItem,
  navigate,
}) => {
  const parsedData = data.map((d: IItemModel) => ({
    name: d.name,
    extension: d.extension,
    isDirectory: d.isDirectory,
    size: d.size,
    mimeType: d.mimeType,
    modified: d.modified,
  }));

  const { values, setFieldValue } = useFormikContext<IItemModel>();

  return (
    <styled.ClipDirectoryTable>
      <GridTable
        paging={{ showPaging: false }}
        columns={clipDirectoryColumns(deleteItem, selectItem, downloadItem, attachItem, values)}
        header={ClipDirectoryFilter}
        data={parsedData!!}
        className="file-table"
        onRowClick={(row) => navigate(row.original)}
      ></GridTable>
    </styled.ClipDirectoryTable>
  );
};
