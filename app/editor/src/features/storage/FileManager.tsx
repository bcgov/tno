import { FileExplorer } from 'features/storage/FileExplorer';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useApp, useStorage, useWorkOrders } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  Col,
  Error,
  FieldSize,
  getDirectoryName,
  IDirectoryModel,
  Row,
  Show,
  Spinner,
  Text,
} from 'tno-core';

import { defaultDirectory } from './constants';
import { IFileItem, IStream } from './interfaces';
import * as styled from './styled';

export interface IFileManagerProps {
  /** The data location Id */
  locationId?: number;
  /** Whether show other locations */
  showLocations?: boolean;
  /** The initial path to load */
  path?: string;
  /** Pass the clip errors back to the content form */
  setClipErrors: (errors: string) => void;
  /** Event - attach to content */
  onAttach?: (item: IFileItem) => void;
  /** Event navigate to path */
  onNavigate?: (locationId: number, path: string) => void;
}

/**
 * FileManager component provides a way to perform actions on files, including creating and joining clips.
 * @param param0 Component properties.
 * @returns Component
 */
export const FileManager: React.FC<IFileManagerProps> = ({
  locationId = 0,
  showLocations = false,
  path = '',
  setClipErrors,
  onAttach,
  onNavigate,
}) => {
  const [app] = useApp();
  const storageApi = useStorage();
  const navigate = useNavigate();
  const [, { requestFile }] = useWorkOrders();

  const videoRef = React.useRef<HTMLVideoElement>(null);
  const directory = getDirectoryName(path) ?? '';
  const creatingClip = app.requests.some((r) => r.url === 'storage-clip');
  const creatingJoin = app.requests.some((r) => r.url === 'storage-join');

  // TODO: Hardcoding the folder location isn't ideal as the API may be configured differently.
  const [folder, setFolder] = React.useState<IDirectoryModel>(defaultDirectory);
  const [stream, setStream] = React.useState<IStream>();
  const [item, setItem] = React.useState<IFileItem>();
  const [start, setStart] = React.useState<string>('');
  const [end, setEnd] = React.useState<string>('');
  const [prefix, setPrefix] = React.useState<string>('');
  const [error, setError] = React.useState<string>('');
  // An actionPath ensures the action only updates the folder state if it's the same folder.
  const [actionPath, setActionPath] = React.useState<string>('');

  React.useEffect(() => {
    setClipErrors(error);
  }, [error, setClipErrors]);

  React.useEffect(() => {
    if (!!stream && !!videoRef.current) {
      videoRef.current.src = stream.url;
    }
  }, [stream, videoRef]);

  React.useEffect(() => {
    // Extract any filename in the path, we only want the folder.
    storageApi.getDirectory(locationId, directory).then((folder) => {
      if (!!folder) setFolder(folder);
    });
  }, [directory, storageApi, locationId]);

  const deleteFile = async (item: IFileItem) => {
    // TODO: Deleting files should be managed by permissions.
    await storageApi.delete(
      locationId,
      item?.isDirectory ? directory : `${directory}/${item?.name}`,
    );
    setFolder({
      ...folder,
      items: folder.items.filter((i) => i.name !== item?.name),
    });
    toast.success(`${item?.name} has been deleted`);
  };

  const playFile = (item?: IFileItem) => {
    setItem(item);
    if (!!item)
      storageApi.stream(item.locationId, `${item.path}/${item.name}`).then((result) => {
        const mimeType = item.mimeType ?? 'video/mp4';
        setStream(
          !!result
            ? {
                url: result,
                type: mimeType,
              }
            : undefined,
        );
      });
    else setStream(undefined);
  };

  const createClip = async () => {
    if (!!item) {
      if (!start || !end) {
        setError('The clip start time and clip end time must both be set.'); // TODO: Validation needs to be extracted into a yup schema.
      } else if (parseInt(start) >= parseInt(end)) {
        setError('The clip start time must be before the clip end time.');
      } else if (prefix === '') {
        setError('Filename is a required field.');
      } else {
        setError('');
        try {
          setActionPath(path);
          await storageApi
            .clip(item.locationId, `${item.path}/${item.name}`, start, end, prefix)
            .then((item) => {
              if (actionPath === path) {
                setFolder({ ...folder, items: [...folder.items, item] });
                setStart('');
                setEnd('');
              }
            });
        } catch {
          // Ignore error it's already handled.
        } finally {
          setActionPath('');
        }
      }
    }
  };

  const joinClips = async () => {
    if (!!item) {
      try {
        setActionPath(path);
        await storageApi.join(item.locationId, `${item.path}/${item.name}`, prefix).then((file) => {
          if (actionPath === path) {
            setFolder({ ...folder, items: [...folder.items, file] });
            setStart('');
            setEnd('');
            storageApi.stream(item.locationId, `${item.path}/${file.name}`).then((result) => {
              const mimeType = item.mimeType ?? 'video/mp4';
              setStream(
                !!result
                  ? {
                      url: result,
                      type: mimeType,
                    }
                  : undefined,
              );
            });
          }
        });
      } catch {
        // Ignore error it's already handled.
      } finally {
        setActionPath('');
      }
    }
  };

  const downloadFile = async (item: IFileItem) => {
    const fullPath = `${directory}/${item.name}`;
    if (item.isLocal) {
      await toast.promise(storageApi.download(locationId, fullPath), {
        pending: 'Downloading file',
        success: 'Download complete',
        error: 'Download failed',
      });
    } else {
      await requestFile(locationId, fullPath);
      toast.info(`Request has been made to copy '${item.name}' from remote location.`);
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

  const selectFile = (item: IFileItem) => {
    setItem(item);
    setStream(undefined);
  };

  return (
    <styled.FileManager>
      <FileExplorer
        folder={folder}
        path={path}
        onPlay={playFile}
        onAttach={onAttach}
        onSelect={selectFile}
        onNavigate={(locationId, path) => {
          setItem(undefined);
          setStream(undefined);
          if (!!onNavigate) onNavigate(locationId, path);
          else navigate(`/storage/locations/${locationId}?path=${path}`);
        }}
        onDelete={deleteFile}
        onDownload={downloadFile}
        locationId={locationId}
        showLocations={showLocations}
      />
      <Show visible={!!stream}>
        <Row justifyContent="center">
          <Col className="video">
            <Row>
              <video ref={videoRef} controls>
                HTML5 Video is required for this example
              </video>
            </Row>
            <Row justifyContent="center">{<Error error={error} />}</Row>
          </Col>
          <Row className="video-buttons" justifyContent="center">
            <Col>
              <label>Start: {getTimePoint(start)}</label>
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
              <label>End: {getTimePoint(end)}</label>
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
              <Text
                name="prefix"
                width={FieldSize.Small}
                label="Filename"
                className="prefix"
                onChange={(e) => {
                  setPrefix(e.target.value);
                }}
                disabled={!!app.requests.length}
              />
            </Col>
            <Col>
              <Button
                className="create-clip"
                onClick={() => {
                  createClip();
                }}
                variant={ButtonVariant.secondary}
                disabled={!prefix || creatingClip || creatingJoin}
              >
                Create Clip
                <Show visible={creatingClip}>
                  <Spinner size="1em" />
                </Show>
              </Button>
            </Col>
            <Col>
              <Button
                className="create-clip"
                onClick={() => {
                  joinClips();
                }}
                variant={ButtonVariant.secondary}
                disabled={!prefix || creatingClip || creatingJoin}
              >
                Join Clips
                <Show visible={creatingJoin}>
                  <Spinner size="1em" />
                </Show>
              </Button>
            </Col>
          </Row>
        </Row>
      </Show>
    </styled.FileManager>
  );
};
