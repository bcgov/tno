import { useFormikContext } from 'formik';
import { useTooltips } from 'hooks';
import { IItemModel } from 'hooks/api-editor';
import React from 'react';
import { GridTable } from 'tno-core';

import { ClipDirectoryFilter } from './ClipDirectoryFilter';
import { clipDirectoryColumns } from './constants';
import * as styled from './styled';

export interface IClipDirectoryTableProps {
  /** the data to be displayed in the table */
  data: IItemModel[];
  /** Event when the delete button is clicked */
  onDelete: (item: IItemModel) => void;
  /** Event when row is clicked */
  onSelect: (item: IItemModel) => void;
  /** Event when the download button is clicked */
  onDownload: (item: IItemModel) => void;
  /** Event when the attach button is clicked */
  onAttach: (item: IItemModel) => void;
  /** Event when the navigate button is clicked */
  navigate: (item: IItemModel) => void;
}

/**
 * Component provides a table to display directory listings.
 * @param param0 Parameters for component
 * @returns Component
 */
export const ClipDirectoryTable: React.FC<IClipDirectoryTableProps> = ({
  data,
  onDelete,
  onSelect,
  onDownload,
  onAttach,
  navigate,
}) => {
  const { values } = useFormikContext<IItemModel>();

  useTooltips();

  return (
    <styled.ClipDirectoryTable>
      <GridTable
        data={data}
        className="file-table"
        paging={{ showPaging: false }}
        columns={clipDirectoryColumns(onDelete, onSelect, onDownload, onAttach, values)}
        header={ClipDirectoryFilter}
        sorting={{ sortBy: [{ id: 'name', desc: true }] }}
        getRowId={(content) => content.name.toString()}
        onRowClick={(row) => {
          navigate(row.original);
        }}
      ></GridTable>
    </styled.ClipDirectoryTable>
  );
};
