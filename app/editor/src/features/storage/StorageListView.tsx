import { Upload } from 'components/upload';
import { IFolderModel, IItemModel } from 'hooks/api-editor';
import React from 'react';
import { FaCloudDownloadAlt, FaPhotoVideo, FaPlay, FaRegFolder, FaTrash } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useStorage } from 'store/hooks';
import { Button, Col, Row, Show, Text } from 'tno-core';
import { useUpload } from 'upload-context/UploadContext';

import { defaultFolder } from './constants';
import * as styled from './styled';

export const StorageListView: React.FC = (props) => {
  const storage = useStorage();
  const videoRef = React.useRef<HTMLVideoElement>(null);

  const [path, setPath] = React.useState('');
  const [folder, setFolder] = React.useState<IFolderModel>(defaultFolder);
  const [streamUrl, setStreamUrl] = React.useState<string>();
  const [item, setItem] = React.useState<IItemModel>();
  const [file, setFile] = React.useState<File>();

  const { upload } = useUpload();

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

  const selectItem = (item?: IItemModel) => {
    setItem(item);
    setStreamUrl(
      !!item ? `/api/editor/storage/stream?path=${folder.path}/${item.name}` : undefined,
    );
  };

  const deleteItem = async (item: IItemModel) => {
    try {
      await storage.delete(`${folder.path}/${item.name}`);
      selectItem();
      setFolder({ ...folder, items: folder.items.filter((i) => i.name !== item.name) });
      toast.success(`${item.name} has been deleted`);
    } catch {
      // Ignore error a toast would have already been displayed with the error.
    }
  };

  const items = folder.items.map((i) => {
    if (i.isDirectory)
      return (
        <li
          key={i.name}
          onClick={() => {
            selectItem();
            setPath(`${folder.path}/${i.name}`);
          }}
        >
          <div>
            <FaRegFolder />
            <span className="navigate">{i.name}</span>
          </div>
        </li>
      );
    return (
      <li key={i.name}>
        <Row nowrap>
          <Row flex="2 1 0">
            <FaPhotoVideo />
            <span className="file" onClick={() => setItem(i)}>
              {i.name}
            </span>
          </Row>
          <Row flex="1 1 0">
            <FaPlay className="stream" title="watch/listen" onClick={() => selectItem(i)} />
            <FaCloudDownloadAlt
              className="download"
              title="download"
              onClick={() => storage.download(`${folder.path}/${i.name}`)}
            />
            <FaTrash className="delete" title="delete" onClick={() => deleteItem(i)} />
          </Row>
        </Row>
      </li>
    );
  });

  const navPath = !folder.path ? (
    <div>/</div>
  ) : (
    <div>
      <b
        onClick={() => {
          selectItem();
          setPath(path.split('/').slice(0, -1).join('/'));
        }}
      >
        ...
      </b>
      {folder.path}
    </div>
  );

  return (
    <styled.StorageListView>
      <div>
        <p>
          Storage access provides a way to manage audio/video files being uploaded and streamed.
        </p>
      </div>
      <Row alignContent="flex-start">
        <Col flex="1 1 0">
          <div className="path">{navPath}</div>
          <ul className="folder">
            <Show visible={!!items.length}>{items}</Show>
            <Show visible={!items.length}>
              <span>No items found</span>
            </Show>
          </ul>
        </Col>
        <Col flex="1 1 0">
          <Row>
            <Upload
              id="upload"
              name="file"
              file={file}
              verifyDelete={false}
              onSelect={(e) => {
                const file = !!e.target?.files?.length ? e.target.files[0] : undefined;
                setFile(file);
              }}
            />
            <Button onClick={() => upload(file!)} disabled={!file}>
              Upload
            </Button>
          </Row>
          <Show visible={!!item}>
            <div className="file-info">
              <Text name="name" label="Name" value={item?.name} disabled />
              <Text
                name="size"
                label="Size"
                value={!!item?.size ? `${item.size / 1000000} MB` : '0 MB'}
                disabled
              />
            </div>
          </Show>
        </Col>
      </Row>
      <Col className="video" alignItems="stretch">
        <video ref={videoRef} className={!streamUrl ? 'hidden' : ''} controls>
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
    </styled.StorageListView>
  );
};
