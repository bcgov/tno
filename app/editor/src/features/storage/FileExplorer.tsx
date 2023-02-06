import { Breadcrumb } from 'components/breadcrumb';
import { Modal } from 'components/modal';
import { ToggleGroup } from 'components/toggle-group';
import { IFolderModel, useModal, useTooltips } from 'hooks';
import { IItemModel } from 'hooks/api-editor';
import React from 'react';
import { useLookup } from 'store/hooks';
import { Col, GridTable, Row, Show } from 'tno-core';
import { getDirectoryName } from 'utils';

import { fileExplorerColumns } from './constants';
import { FileExplorerFilter } from './FileExplorerFilter';
import { IFileItem } from './interfaces';
import * as styled from './styled';

export interface IFileExplorerProps {
  /** The initial location Id */
  locationId?: number;
  /** The initial path to load */
  path?: string;
  /** The folder to display */
  folder: IFolderModel;
  /** Whether show other locations */
  showLocations?: boolean;
  /** Event when the delete button is clicked */
  onDelete?: (item: IFileItem) => void;
  /** Event play button is clicked */
  onPlay?: (item?: IFileItem) => void;
  /** Event when the download button is clicked */
  onDownload?: (item: IFileItem) => void;
  /** Event when the attach button is clicked */
  onAttach?: (item: IFileItem) => void;
  /** Event row is clicked */
  onSelect?: (item: IFileItem) => void;
  /** Event navigate to path */
  onNavigate?: (locationId: number, path: string) => void;
}

/**
 * FileExplorer component provides a way to navigate folders and files on the API and in remote locations.
 * @param param0 Parameters for component
 * @returns Component
 */
export const FileExplorer: React.FC<IFileExplorerProps> = ({
  locationId = 0,
  showLocations = false,
  path = '',
  folder,
  onDelete,
  onPlay,
  onDownload,
  onAttach,
  onSelect,
  onNavigate,
}) => {
  const [{ dataLocations }] = useLookup();
  const { toggle, isShowing } = useModal();
  useTooltips();

  const [item, setItem] = React.useState<IItemModel>();

  const directory = getDirectoryName(path) ?? '';
  const isStorage = window.location.href.includes('/storage');
  const locations = dataLocations.map((i) => ({
    id: i.id,
    label: i.name,
    data: i,
    onClick: () => {
      onNavigate?.(i.id, '');
    },
  }));

  const navigate = (item: IItemModel) => {
    if (item?.isDirectory) {
      onNavigate?.(locationId, `${directory}/${item?.name}`);
    }
    onSelect?.({ ...item, locationId, path });
  };

  return (
    <styled.FileExplorer>
      <Row>
        <Show visible={showLocations}>
          <ToggleGroup
            label="Location"
            defaultSelected={locationId}
            options={locations}
            alignItems="center"
          />
        </Show>
        <Breadcrumb
          label="Path"
          path={directory}
          onNavigate={(path) => {
            onNavigate?.(locationId, path);
          }}
        />
      </Row>
      <Row>
        <Col flex="1 1 100%">
          <div className="file-table">
            <GridTable
              enableRowSelect={false}
              data={folder.items}
              className="file-table"
              paging={{ showPaging: false }}
              columns={fileExplorerColumns({
                onSelect: navigate,
                onDelete: (item) => {
                  setItem(item);
                  toggle();
                },
                onPlay: (item) => onPlay?.({ ...item, locationId, path: directory }),
                onDownload: (item) => onDownload?.({ ...item, locationId, path: directory }),
                onAttach: (item) => onAttach?.({ ...item, locationId, path: directory }),
                isStorage,
              })}
              header={FileExplorerFilter}
              sorting={{ sortBy: [{ id: 'name', desc: true }] }}
              getRowId={(content) => content.name.toString()}
            ></GridTable>
          </div>
        </Col>
      </Row>
      {/* Modal to appear when deleting a file */}
      <Modal
        isShowing={isShowing}
        hide={toggle}
        type="delete"
        headerText="Confirm Delete"
        body={`Do you want to delete '${path}/${item?.name}'?`}
        confirmText="Yes"
        cancelText="No"
        onConfirm={async () => {
          try {
            if (!!item) {
              onPlay?.();
              onDelete?.({ ...item, locationId, path });
            }
          } finally {
            toggle();
          }
        }}
      />
    </styled.FileExplorer>
  );
};
