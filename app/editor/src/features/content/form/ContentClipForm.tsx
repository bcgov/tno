import { Modal } from 'components/modal';
import { useFormikContext } from 'formik';
import { IFileReferenceModel, IFolderModel, IItemModel } from 'hooks/api-editor';
import { useModal } from 'hooks/modal';
import React from 'react';
import { toast } from 'react-toastify';
import { useContent, useStorage } from 'store/hooks';
import { Button, ButtonVariant, Col, Row, Text } from 'tno-core';

import { defaultFolder } from '../../storage/constants';
import { ClipDirectoryTable } from './ClipDirectoryTable';
import { IContentForm } from './interfaces';
import * as styled from './styled';
import { toForm } from './utils';

export interface IContentClipFormProps {
  /** The content currently being viewed. */
  content: IContentForm;
  /** A way to update the content when attaching a file. */
  setContent: (content: IContentForm) => void;
  /** The initial path to load */
  path?: string;
}

/**
 * The component to be displayed when the clips tab is selected from the content form.
 * @param param0 Component properties.
 * @returns Component
 */
export const ContentClipForm: React.FC<IContentClipFormProps> = ({
  content,
  setContent,
  path: initPath,
}) => {
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const { toggle, isShowing } = useModal();
  const storageApi = useStorage();
  const [, contentApi] = useContent();

  const videoRef = React.useRef<HTMLVideoElement>(null);

  // TODO: Hardcoding the folder location isn't ideal as the API may be configured differently.
  const defaultPath = `/clip/${content.source}`;
  const [path, setPath] = React.useState(initPath ?? defaultPath);
  const [folder, setFolder] = React.useState<IFolderModel>(defaultFolder);
  const [streamUrl, setStreamUrl] = React.useState<string>();
  const [item, setItem] = React.useState<IItemModel>();
  const [start, setStart] = React.useState<string>('');
  const [end, setEnd] = React.useState<string>('');
  const [currFile, setCurrFile] = React.useState<string>('');
  const [prefix, setPrefix] = React.useState<string>('');

  React.useEffect(() => {
    // Many data source locations will not have an existing clip folder.
    // Check if it exists before attempting to load it.
    const response = defaultPath === path ? storageApi.folderExists(path) : Promise.resolve(true);
    response.then((exists) => {
      var directory = exists ? path : '/';
      storageApi.getFolder(directory).then((directory) => {
        setFolder(directory);
      });
    });
  }, [defaultPath, path, storageApi]);

  React.useEffect(() => {
    if (!!streamUrl && !!videoRef.current) {
      videoRef.current.src = streamUrl;
    }
  }, [streamUrl, videoRef]);

  const onDelete = async (item: IItemModel) => {
    setItem(item);
    toggle();
  };

  const onDownload = async (item: IItemModel) => {
    await storageApi.download(`${folder.path}/${item.name}`);
  };

  const onAttach = async (item: IItemModel) => {
    if (values.id === 0) {
      // Add a reference to the file so that it can be copied to the API when the content is saved.
      setFieldValue('fileReferences', [
        {
          contentType: item.mimeType,
          fileName: item.name,
          path: `${folder.path}/${item.name}`,
          size: item.size,
          isUploaded: false,
        } as IFileReferenceModel,
      ]);
    } else {
      await contentApi.attach(content.id, `${folder.path}/${item.name}`).then((data) => {
        setContent(toForm(data));
        toast.success('Attachment added to this snippet.');
      });
    }
  };

  const onSelect = (item?: IItemModel) => {
    setItem(item);
    setCurrFile(!!item ? item.name : '');
    setStreamUrl(
      !!item ? `/api/editor/storage/stream?path=${folder.path}/${item.name}` : undefined,
    );
  };

  const createClip = async () => {
    if (!start || !end) {
      toast.error('The clip start time and clip end time must both be set.');
    } else if (parseInt(start) >= parseInt(end)) {
      toast.error('The clip start time must be before the clip end time.');
    } else if (prefix === '') {
      toast.error('Prefix is a required field.');
    } else {
      await storageApi.clip(`${folder.path}/${currFile}`, start, end, prefix).then((item) => {
        setFolder({ ...folder, items: [...folder.items, item] });
        setStart('');
        setEnd('');
      });
    }
  };

  const joinClips = async () => {
    await storageApi.join(`${folder.path}/${currFile}`, prefix).then((item) => {
      setItem(item);
      setFolder({ ...folder, items: [...folder.items, item] });
      setStart('');
      setEnd('');
      setCurrFile(!!item ? item.name : '');
      setStreamUrl(
        !!item ? `/api/editor/storage/stream?path=${folder.path}/${item.name}` : undefined,
      );
    });
  };

  const setStartTime = () => {
    setStart(!!videoRef.current ? videoRef.current?.currentTime.toFixed() : '');
  };

  const setEndTime = () => {
    setEnd(!!videoRef.current ? videoRef.current?.currentTime.toFixed() : '');
  };

  const navigate = (item?: IItemModel) => {
    if (item?.isDirectory) {
      setPath(`${folder.path}/${item?.name}`);
    }
  };

  const navigateUp = () => {
    var path = folder.path.split('/').filter((v) => !!v);
    if (path.length <= 1) setPath('/');
    else setPath(path.slice(0, -1).join('/'));
  };

  return (
    <styled.ContentClipForm>
      <div>
        <Button className="navigate-up" onClick={() => navigateUp()} name="navigateUp">
          Directory: {folder.path}
        </Button>
      </div>
      <Row>
        <Col flex="1 1 100%">
          <div className="file-table">
            <ClipDirectoryTable
              data={folder.items}
              onDelete={onDelete}
              onSelect={onSelect}
              onDownload={onDownload}
              onAttach={onAttach}
              navigate={navigate}
            />
          </div>
        </Col>
      </Row>
      <div className={!streamUrl ? 'hidden' : ''}>
        <Row>
          <Col className="video" alignItems="stretch">
            <video ref={videoRef} controls>
              <source type="audio/m4a" />
              <source type="audio/flac" />
              <source type="audio/mp3" />
              <source type="audio/mp4" />
              <source type="audio/wav" />
              <source type="audio/wma" />
              <source type="audio/aac" />
              <source type="video/wmv" />
              <source type="video/mov" />
              <source type="video/mpeg" />
              <source type="video/mpg" />
              <source type="video/avi" />
              <source type="video/mp4" />
              <source type="video/gif" />
              HTML5 Video is required for this example
            </video>
          </Col>
        </Row>
        <Row className="video-buttons">
          <Col>
            <p className="start-end">Start: {start}</p>
            <Button
              onClick={() => {
                setStartTime();
              }}
              variant={ButtonVariant.secondary}
            >
              Set Start
            </Button>
          </Col>
          <Col>
            <p className="start-end">End: {end}</p>
            <Button
              onClick={() => {
                setEndTime();
              }}
              variant={ButtonVariant.secondary}
            >
              Set End
            </Button>
          </Col>
          <Col>
            <p className="start-end">Prefix:</p>
            <Text
              name="prefix"
              label=""
              className="prefix"
              onChange={(e) => {
                setPrefix(e.target.value);
              }}
            />
          </Col>
          <Col>
            <Button
              className="create-clip"
              onClick={() => {
                createClip();
              }}
              variant={ButtonVariant.secondary}
            >
              Create Clip
            </Button>
          </Col>
          <Col>
            <Button
              className="create-clip"
              onClick={() => {
                joinClips();
              }}
              variant={ButtonVariant.secondary}
            >
              Join Clips
            </Button>
          </Col>
          {/* Modal to appear when removing a file */}
          <Modal
            isShowing={isShowing}
            hide={toggle}
            type="delete"
            headerText="Confirm Delete"
            body={`Are you sure you want to delete this ${item?.isDirectory ? 'folder' : 'file'}?`}
            confirmText="Yes, Remove It"
            onConfirm={async () => {
              try {
                // TODO: Only certain users should be allowed to delete certain files/folders.
                await storageApi.delete(
                  item?.isDirectory ? folder.path : `${folder.path}/${item?.name}`,
                );
                onSelect();
                setFolder({
                  ...folder,
                  items: folder.items.filter((i) => i.name !== item?.name),
                });
                toast.success(`${item?.name} has been deleted`);
              } finally {
                toggle();
              }
            }}
          />
        </Row>
      </div>
    </styled.ContentClipForm>
  );
};
