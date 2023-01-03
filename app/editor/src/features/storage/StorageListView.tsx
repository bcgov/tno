import { FormikForm } from 'components/formik';
import { ContentClipForm } from 'features/content';
import { defaultFormValues } from 'features/content/form/constants';
import { IContentForm } from 'features/content/form/interfaces';
import { ContentTypeName, useCombinedView, useTooltips } from 'hooks';
import { IFolderModel, IItemModel } from 'hooks/api-editor';
import React from 'react';
import {
  FaCloudDownloadAlt,
  FaPhotoVideo,
  FaPlay,
  FaRegFolder,
  FaRegImage,
  FaTrash,
} from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useStorage } from 'store/hooks';
import { Row, Show } from 'tno-core';
import { isNotVideoOrAudio } from 'utils';

import { defaultFolder } from './constants';
import * as styled from './styled';

export const StorageListView: React.FC = (props) => {
  const storage = useStorage();
  const videoRef = React.useRef<HTMLVideoElement>(null);

  const [path, setPath] = React.useState('');
  const [folder, setFolder] = React.useState<IFolderModel>(defaultFolder);
  const [streamUrl, setStreamUrl] = React.useState<string>();
  const [, setItem] = React.useState<IItemModel>();

  const [, setClipErrors] = React.useState<string>('');
  const { formType } = useCombinedView(ContentTypeName.Snippet);
  const [contentType] = React.useState(formType ?? ContentTypeName.Snippet);
  const [form, setForm] = React.useState<IContentForm>({
    ...defaultFormValues(contentType),
  });

  useTooltips();

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
            {isNotVideoOrAudio(i.name) ? <FaRegImage /> : <FaPhotoVideo />}
            <span className="file" onClick={() => setItem(i)}>
              {i.name}
            </span>
          </Row>
          <Row flex="1 1 0">
            {!isNotVideoOrAudio(i.name) && (
              <FaPlay
                className="stream"
                data-for="main-tooltip"
                data-tip="watch/listen"
                onClick={() => selectItem(i)}
              />
            )}
            <FaCloudDownloadAlt
              className="download"
              data-for="main-tooltip"
              data-tip="download"
              onClick={() => storage.download(`${folder.path}/${i.name}`)}
            />
            <FaTrash
              className="delete"
              data-for="main-tooltip"
              data-tip="delete"
              onClick={() => deleteItem(i)}
            />
          </Row>
        </Row>
      </li>
    );
  });

  return (
    <styled.StorageListView>
      <Show visible={!!items.length}>
        <FormikForm onSubmit={() => {}} initialValues={form}>
          <ContentClipForm content={form} setContent={setForm} setClipErrors={setClipErrors} />
        </FormikForm>
      </Show>
    </styled.StorageListView>
  );
};
