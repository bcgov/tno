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

import {
  Col,
  isImageFile,
  isVideoOrAudioFile,
  Row,
  Show,
  type IItemModel,
  type ITableHookColumn,
} from '@/shared';

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
): Array<ITableHookColumn<IItemModel>> => [
  {
    label: <div className="list-icon"></div>,
    accessor: 'isDirectory',
    showSort: false,
    width: 20,
    cell: ({ row }) => (
      <div>
        <div className={row.original.isDirectory ? 'hidden' : 'center'}>
          {isVideoOrAudioFile(row.original.name) ? (
            <FaPhotoVideo className="fa-lg" />
          ) : isImageFile(row.original.name) ? (
            <FaRegImage className="fa-lg" />
          ) : (
            <FaRegFile className="fa-lg" />
          )}
        </div>
        <div className={row.original.isDirectory ? 'center' : 'hidden'}>
          <FaRegFolder className="fa-lg" />
        </div>
      </div>
    ),
  },
  {
    label: <div className="center">Filename</div>,
    accessor: 'name',
    sort: 'name',
    width: 125,
    cell: (cell) => (
      <div className="ft-row">
        <span
          className={cell.original.isDirectory ? 'link' : ''}
          onClick={() => options?.onSelect?.(cell.original)}
        >
          {cell.original.name}
        </span>
      </div>
    ),
  },
  {
    label: <div className="center">Size</div>,
    accessor: 'size',
    sort: 'size',
    width: 35,
    cell: ({ original }) => (
      <div className="ft-row">
        {original.size ? `${(original.size / 1000000).toFixed(2)} MB` : ''}
      </div>
    ),
  },
  {
    label: <div className="center">Modified</div>,
    accessor: 'modified',
    sort: 'modified',
    width: 40,
    cell: ({ original }) => (
      <div className="ft-row">{moment(original.modified).format('DD-MM-yy hh:mm:ss')}</div>
    ),
  },
  {
    label: <div>Actions</div>,
    showSort: false,
    width: 30,
    cell: ({ row }) => (
      <Row className={`file-actions ${row.original.isDirectory && 'directory'}`} wrap="nowrap">
        <Show visible={typeof options?.onPlay === 'function'}>
          <Col>
            <Show visible={isVideoOrAudioFile(row.original.name) && row.original.isLocal}>
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
