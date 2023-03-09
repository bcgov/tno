import { IItemModel } from 'hooks/api-editor';
import moment from 'moment';
import {
  FaCopy,
  FaFileDownload,
  FaPaperclip,
  FaPhotoVideo,
  FaPlay,
  FaRegFile,
  FaRegFolder,
  FaRegImage,
  FaTrash,
} from 'react-icons/fa';
import { Column, UseSortByColumnOptions } from 'react-table';
import { Col, Row, Show } from 'tno-core';
import { isImageFile, isVideoOrAudioFile } from 'utils';

interface IColumnOptions {
  isStorage?: boolean;
  onSelect?: (item: IItemModel) => void;
  onDelete?: (item: IItemModel) => void;
  onPlay?: (item: IItemModel) => void;
  onDownload?: (item: IItemModel) => void;
  onAttach?: (item: IItemModel) => void;
}

/** columns located within file for state manipulation */
export const fileExplorerColumns = (
  options?: IColumnOptions,
): (Column<IItemModel> & UseSortByColumnOptions<IItemModel>)[] => [
  {
    id: 'isDirectory',
    Header: () => <div className="list-icon"></div>,
    accessor: 'isDirectory',
    disableSortBy: true,
    Cell: ({ row, value }) => (
      <div>
        <div className={row.values.isDirectory ? 'hidden' : 'center'}>
          {isVideoOrAudioFile(row.values.name) ? (
            <FaPhotoVideo className="fa-lg" />
          ) : isImageFile(row.values.name) ? (
            <FaRegImage className="fa-lg" />
          ) : (
            <FaRegFile className="fa-lg" />
          )}
        </div>
        <div className={row.values.isDirectory ? 'center' : 'hidden'}>
          <FaRegFolder className="fa-lg" />
        </div>
      </div>
    ),
    maxWidth: 20,
  },
  {
    id: 'name',
    Header: () => <div className="center">Filename</div>,
    accessor: 'name',
    Cell: (cell) => (
      <div className="ft-row">
        <span
          className={cell.row.original.isDirectory ? 'link' : ''}
          onClick={() => options?.onSelect?.(cell.row.original)}
        >
          {cell.value}
        </span>
      </div>
    ),
    width: 125,
  },
  {
    id: 'size',
    Header: () => <div className="center">Size</div>,
    accessor: 'size',
    maxWidth: 35,
    Cell: ({ value }) => (
      <div className="ft-row">{!!value ? `${(value / 1000000).toFixed(2)} MB` : ''}</div>
    ),
  },
  {
    id: 'modified',
    Header: () => <div className="center">Modified</div>,
    accessor: 'modified',
    maxWidth: 40,
    Cell: ({ value }) => <div className="ft-row">{moment(value).format('DD-MM-yy hh:mm:ss')}</div>,
  },
  {
    id: 'actions',
    Header: () => <div>Actions</div>,
    accessor: 'isDirectory',
    maxWidth: 30,
    Cell: ({ row }) => (
      <Row className={`file-actions ${row.original.isDirectory && 'directory'}`} wrap="nowrap">
        <Show visible={typeof options?.onPlay === 'function'}>
          <Col>
            <Show visible={isVideoOrAudioFile(row.values.name) && row.original.isLocal}>
              <FaPlay
                data-tooltip-id="main-tooltip"
                data-tooltip-content="Play"
                onClick={() => options?.onPlay?.(row.original)}
                className={`${row.original.isDirectory && 'hidden'}`}
              />
            </Show>
          </Col>
        </Show>
        <Show visible={typeof options?.onDownload === 'function'}>
          <Col>
            <Show visible={row.original.isLocal}>
              <FaFileDownload
                className={`fa-lg ${row.original.isDirectory && 'hidden'}`}
                data-tooltip-id="main-tooltip"
                data-tooltip-content="Download"
                onClick={() => {
                  options?.onDownload?.(row.original);
                }}
              />
            </Show>
            <Show visible={!row.original.isLocal}>
              <FaCopy
                className={`fa-lg ${row.original.isDirectory && 'hidden'}`}
                data-tooltip-id="main-tooltip"
                data-tooltip-content="Request Copy"
                onClick={() => {
                  options?.onDownload?.(row.original);
                }}
              />
            </Show>
          </Col>
        </Show>
        <Show visible={typeof options?.onAttach === 'function'}>
          <Col>
            <Show visible={row.original.isLocal}>
              <FaPaperclip
                className={`fa-lg ${(row.original.isDirectory || options?.isStorage) && 'hidden'}`}
                data-tooltip-id="main-tooltip"
                data-tooltip-content="Attach to snippet"
                onClick={() => {
                  options?.onAttach?.(row.original);
                }}
              />
            </Show>
          </Col>
        </Show>
        <Show visible={typeof options?.onDelete === 'function'}>
          <Col>
            <FaTrash
              className="delete fa-lg"
              data-tooltip-id="main-tooltip"
              data-tooltip-content="Delete"
              onClick={() => {
                options?.onDelete?.(row.original);
              }}
            />
          </Col>
        </Show>
      </Row>
    ),
  },
];
