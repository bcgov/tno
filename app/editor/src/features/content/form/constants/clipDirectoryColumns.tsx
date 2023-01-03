import { IItemModel } from 'hooks/api-editor';
import moment from 'moment';
import {
  FaCloudDownloadAlt,
  FaPaperclip,
  FaPhotoVideo,
  FaPlay,
  FaRegFolder,
  FaRegImage,
  FaTrash,
} from 'react-icons/fa';
import { Column, UseSortByColumnOptions } from 'react-table';
import { Col, Row } from 'tno-core';
import { isNotVideoOrAudio } from 'utils';

/** columns located within file for state manipulation */
export const clipDirectoryColumns = (
  onDelete: Function,
  onSelect: Function,
  onDownload: Function,
  onAttach: Function,
  values: IItemModel,
  isStorage?: boolean,
): (Column<IItemModel> & UseSortByColumnOptions<IItemModel>)[] => [
  {
    id: 'isDirectory',
    Header: () => <div className="list-icon"></div>,
    accessor: 'isDirectory',
    disableSortBy: true,
    Cell: ({ row, value }) => (
      <div>
        <div className={row.values.isDirectory ? 'hidden' : 'center'}>
          {!isNotVideoOrAudio(row.values.name) ? (
            <FaPhotoVideo className="fa-lg" />
          ) : (
            <FaRegImage className="fa-lg" />
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
    Cell: ({ value }) => <div className="ft-row">{value}</div>,
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
        <Col>
          {!isNotVideoOrAudio(row.values.name) && (
            <FaPlay
              data-for="main-tooltip"
              data-tip="watch/listen/edit"
              onClick={() => onSelect(row.original)}
              className={`${row.original.isDirectory && 'hidden'}`}
            />
          )}
        </Col>
        <Col>
          <FaCloudDownloadAlt
            className={`fa-lg ${row.original.isDirectory && 'hidden'}`}
            data-for="main-tooltip"
            data-tip="download"
            onClick={() => {
              onDownload(row.original);
            }}
          />
        </Col>
        <Col>
          <FaPaperclip
            className={`fa-lg ${(row.original.isDirectory || isStorage) && 'hidden'}`}
            data-for="main-tooltip"
            data-tip="Attach to snippet"
            onClick={() => {
              onAttach(row.original);
            }}
          />
        </Col>
        <Col>
          <FaTrash
            className="delete fa-lg"
            data-for="main-tooltip"
            data-tip="Delete"
            onClick={() => {
              onDelete(row.original);
            }}
          />
        </Col>
      </Row>
    ),
  },
];
