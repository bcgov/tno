import { IItemModel } from 'hooks/api-editor';
import {
  FaCloudDownloadAlt,
  FaPaperclip,
  FaPhotoVideo,
  FaPlay,
  FaRegFolder,
  FaTrash,
} from 'react-icons/fa';
import { Column } from 'react-table';

/** columns located within file for state manipulation */
export const clipDirectoryColumns = (
  deleteItem: Function,
  selectItem: Function,
  downloadItem: Function,
  attachItem: Function,
  values: IItemModel,
): Column<IItemModel>[] => [
  {
    id: 'isDirectory',
    Header: () => <div className="list-icon"></div>,
    accessor: 'isDirectory',
    Cell: ({ row, value }) => (
      <div>
        <div className={row.values.isDirectory ? 'hidden' : 'center'}>
          <FaPhotoVideo className="fa-lg" />
        </div>
        <div className={row.values.isDirectory ? 'center' : 'hidden'}>
          <FaRegFolder className="fa-lg" />
        </div>
      </div>
    ),
    width: 20,
  },
  {
    id: 'name',
    Header: () => <div className="center">Filename</div>,
    accessor: 'name',
    Cell: ({ value }) => <div className="ft-row">{value}</div>,
  },
  {
    id: 'size',
    Header: () => <div className="center">Size</div>,
    accessor: 'size',
    Cell: ({ value }) => <div className="ft-row">{!!value ? `${value / 1000000} MB` : ''}</div>,
  },
  {
    id: 'modified',
    Header: () => <div className="center">Modified</div>,
    accessor: 'modified',
    Cell: ({ value }) => <div className="ft-row">{value}</div>,
  },
  {
    id: 'actions',
    Header: () => <div>Actions</div>,
    accessor: 'isDirectory',
    width: 80,
    Cell: ({ row, data }: any) => (
      <div className={row.values.isDirectory ? 'hidden' : 'center'}>
        <FaPlay className="stream" title="watch/listen" onClick={() => selectItem(row.values)} />
        <FaCloudDownloadAlt
          className="download fa-lg"
          title="download"
          onClick={() => {
            downloadItem(row.values);
          }}
        />
        <FaPaperclip
          className="attach fa-lg"
          title="Attach to snippet"
          onClick={() => {
            attachItem(row.values);
          }}
        />
        <FaTrash
          className="delete fa-lg"
          title="Delete"
          onClick={() => {
            deleteItem(row.values);
          }}
        />
      </div>
    ),
  },
];
