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
  onDelete: (item: IItemModel) => void;
  onSelect: (item: IItemModel) => void;
  onDownload: (item: IItemModel) => void;
  onAttach: (item: IItemModel) => void;
  navigate: (item: IItemModel) => void;
}

/** Table used to display directory listing of clip files for the data source */
export const ClipDirectoryTable: React.FC<IClipDirectoryTableProps> = ({
  data,
  onDelete,
  onSelect,
  onDownload,
  onAttach,
  navigate,
}) => {
  const { values } = useFormikContext<IItemModel>();

  return (
    <styled.ClipDirectoryTable>
      <GridTable
        paging={{ showPaging: false }}
        columns={clipDirectoryColumns(onDelete, onSelect, onDownload, onAttach, values)}
        header={ClipDirectoryFilter}
        data={data}
        className="file-table"
        onRowClick={(row) => navigate(row.original)}
      ></GridTable>
    </styled.ClipDirectoryTable>
  );
};
