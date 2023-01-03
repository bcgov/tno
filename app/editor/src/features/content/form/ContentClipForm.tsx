import { Breadcrumb } from 'components/breadcrumb';
import { Error } from 'components/form';
import { Modal } from 'components/modal';
import { useFormikContext } from 'formik';
import { IFileReferenceModel, IFolderModel, IItemModel } from 'hooks/api-editor';
import { useModal } from 'hooks/modal';
import React from 'react';
import { toast } from 'react-toastify';
import { useContent, useStorage } from 'store/hooks';
import { Button, ButtonVariant, Col, FieldSize, Row, Text } from 'tno-core';

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
  /** Pass the clip errors back to the content form */
  setClipErrors: (errors: string) => void;
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
  setClipErrors,
}) => {
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const { toggle, isShowing } = useModal();
  const storageApi = useStorage();
  const [, contentApi] = useContent();

  const videoRef = React.useRef<HTMLVideoElement>(null);

  // TODO: Hardcoding the folder location isn't ideal as the API may be configured differently.
  const defaultPath = `/clip/${content.otherSource}`;
  const [path, setPath] = React.useState(initPath ?? defaultPath);
  const [folder, setFolder] = React.useState<IFolderModel>(defaultFolder);
  const [streamUrl, setStreamUrl] = React.useState<string>();
  const [item, setItem] = React.useState<IItemModel>();
  const [start, setStart] = React.useState<string>('');
  const [end, setEnd] = React.useState<string>('');
  const [currFile, setCurrFile] = React.useState<string>('');
  const [prefix, setPrefix] = React.useState<string>('');
  const [error, setError] = React.useState<string>('');

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
    setClipErrors(error);
  }, [error, setClipErrors]);

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
      setError('The clip start time and clip end time must both be set.');
    } else if (parseInt(start) >= parseInt(end)) {
      setError('The clip start time must be before the clip end time.');
    } else if (prefix === '') {
      setError('Filename is a required field.');
    } else {
      setError('');
      try {
        await storageApi.clip(`${folder.path}/${currFile}`, start, end, prefix).then((item) => {
          setFolder({ ...folder, items: [...folder.items, item] });
          setStart('');
          setEnd('');
        });
      } catch {
        // Ignore error it's already handled.
      }
    }
  };

  const joinClips = async () => {
    try {
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
    } catch {
      // Ignore error it's already handled.
    }
  };

  const getTime = () => {
    return !!videoRef.current ? Math.floor(videoRef.current?.currentTime).toString() : '';
  };

  const getTimePoint = (time: string) => {
    if (time) {
      const currentTime = +time;
      const hours =
        currentTime < 3600
          ? ''
          : Math.floor(currentTime / 3600).toLocaleString(undefined, { minimumIntegerDigits: 2 }) +
            ':';
      const minutes = (Math.floor(currentTime / 60) % 60).toLocaleString(undefined, {
        minimumIntegerDigits: 2,
      });
      const seconds = (currentTime % 60).toLocaleString(undefined, { minimumIntegerDigits: 2 });
      return `${hours}${minutes}:${seconds}`;
    }
    return time;
  };

  const setStartTime = () => {
    setStart(getTime());
  };

  const setEndTime = () => {
    setEnd(getTime());
  };

  const navigate = (item?: IItemModel) => {
    if (item?.isDirectory) {
      setPath(`${folder.path}/${item?.name}`);
    } else if (streamUrl) {
      setStreamUrl(undefined);
    }
  };

  return (
    <styled.ContentClipForm>
      <div>
        <Breadcrumb path={folder.path} setPath={setPath} />
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
      <Row justifyContent="center" className={!streamUrl ? 'hidden' : ''}>
        <Col className="video">
          <Row>
            <video ref={videoRef} controls>
              <source src={streamUrl} type="audio/m4a" />
              <source src={streamUrl} type="audio/flac" />
              <source src={streamUrl} type="audio/mp3" />
              <source src={streamUrl} type="audio/mp4" />
              <source src={streamUrl} type="audio/wav" />
              <source src={streamUrl} type="audio/wma" />
              <source src={streamUrl} type="audio/aac" />
              <source src={streamUrl} type="video/wmv" />
              <source src={streamUrl} type="video/mov" />
              <source src={streamUrl} type="video/mpeg" />
              <source src={streamUrl} type="video/mpg" />
              <source src={streamUrl} type="video/avi" />
              <source src={streamUrl} type="video/mp4" />
              <source src={streamUrl} type="video/gif" />
              <source src={streamUrl} type="video/ogg" />
              <source src={streamUrl} type="video/webm" />
              HTML5 Video is required for this example
            </video>
          </Row>
          <Row justifyContent="center">{<Error error={error} />}</Row>
        </Col>
        <Row className="video-buttons" justifyContent="center">
          <Col>
            <p className="start-end">Start: {getTimePoint(start)}</p>
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
            <p className="start-end">End: {getTimePoint(end)}</p>
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
            <p className="start-end">Filename:</p>
            <Text
              name="prefix"
              width={FieldSize.Small}
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
      </Row>
    </styled.ContentClipForm>
  );
};
