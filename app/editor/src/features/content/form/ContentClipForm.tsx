import { Modal } from 'components/modal';
import { useFormikContext } from 'formik';
import { IFolderModel, IItemModel } from 'hooks/api-editor';
import { useModal } from 'hooks/modal';
import React from 'react';
import { toast } from 'react-toastify';
import { Button, ButtonVariant, Col, Row, Text } from 'tno-core';

import { useStorage } from '../../../store/hooks';
import { defaultFolder } from '../../storage/constants';
import { ClipDirectoryTable } from './ClipDirectoryTable';
import { IContentForm } from './interfaces';
import * as styled from './styled';

export interface IContentClipFormProps {
  content: IContentForm;
}

/**
 * The component to be displayed when the clips tab is selected from the content form.
 * @returns the ContentClipForm
 */
export const ContentClipForm: React.FC<IContentClipFormProps> = ({ content }) => {
  const videoRef = React.useRef<HTMLVideoElement>(null);

  const [path, setPath] = React.useState('/' + content.source);
  const [folder, setFolder] = React.useState<IFolderModel>(defaultFolder);
  const [streamUrl, setStreamUrl] = React.useState<string>();
  const [item, setItem] = React.useState<IItemModel>();
  const [start, setStart] = React.useState<string>('');
  const [end, setEnd] = React.useState<string>('');
  const [clipNbr, setClipNbr] = React.useState<number>(1);
  const [currFile, setCurrFile] = React.useState<string>('');
  const [target, setTarget] = React.useState<string>('');
  const { toggle, isShowing } = useModal();
  const storage = useStorage();

  React.useEffect(() => {
    storage.getFolder(path).then((data) => {
      setFolder(data);
    });
  }, [path, storage]);

  React.useEffect(() => {
    if (!!streamUrl && !!videoRef.current) {
      videoRef.current.src = streamUrl;
    }
  }, [streamUrl, videoRef]);

  const deleteItem = async (item: IItemModel) => {
    try {
      setItem(item);
      toggle();
    } catch {
      // Ignore error a toast would have already been displayed with the error.
    }
  };

  const downloadItem = async (item: IItemModel) => {
    console.log(content);
    await storage.download(`${folder.path}/${item.name}`);
  };

  const attachItem = async (item: IItemModel) => {
    await storage.attach(content.id, `${folder.path}/${item.name}`).then((data) => {
      toast.success('Attachment added to this snippet.');
    });
  };

  const selectItem = (item?: IItemModel) => {
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
    } else if (!target) {
      toast.error('You must provide a target file name.');
    } else {
      setClipNbr(!!clipNbr ? clipNbr + 1 : 1);
      await storage.clip(currFile, folder.path, start, end, clipNbr, target).then((data) => {
        setFolder({ ...data, items: data.items });
        setStart('');
        setEnd('');
      });
    }
  };

  const joinClips = async () => {
    await storage.join(currFile, folder.path, target).then((data) => {
      setFolder({ ...data, items: data.items });
      setClipNbr(1);
      setStart('');
      setEnd('');
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
    var pathComps = folder.path.split('/');
    if (pathComps.length === 3) {
      setPath(pathComps[1]);
    }
  };

  return (
    <styled.ContentClipForm>
      <div>
        <Button className="navigate-up" onClick={() => navigateUp()} name="navigateUp">
          Directory: {folder.path}
        </Button>
      </div>
      <Row>
        <Col>
          <div className="file-table">
            <ClipDirectoryTable
              data={folder.items}
              deleteItem={deleteItem}
              selectItem={selectItem}
              downloadItem={downloadItem}
              attachItem={attachItem}
              navigate={navigate}
            />
          </div>
        </Col>
      </Row>
      <div className={!streamUrl ? 'hidden' : ''}>
        <Row>
          <div className="editing">Editing: {item?.name}</div>
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
          <Col>
            <p className="start-end">Target:</p>
            <Text
              name="target"
              label=""
              className="target"
              onChange={(e) => {
                setTarget(e.target.value);
              }}
            />
            {/* Modal to appear when removing a file */}
            <Modal
              isShowing={isShowing}
              hide={toggle}
              type="delete"
              headerText="Confirm Removal"
              body="Are you sure you want to remove this file?"
              confirmText="Yes, Remove It"
              onConfirm={async () => {
                await storage.delete(`${folder.path}/${item?.name}`);
                selectItem();
                setFolder({ ...folder, items: folder.items.filter((i) => i.name !== item?.name) });
                toast.success(`${item?.name} has been deleted`);
                toggle();
              }}
            />
          </Col>
        </Row>
      </div>
    </styled.ContentClipForm>
  );
};
