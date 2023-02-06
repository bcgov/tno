import { FileManager } from 'features/storage';
import { IFileItem } from 'features/storage/interfaces';
import { useFormikContext } from 'formik';
import { useQuery } from 'hooks';
import { IFileReferenceModel } from 'hooks/api-editor';
import React from 'react';
import { useNavigate } from 'react-router-dom';
import { toast } from 'react-toastify';
import { useContent } from 'store/hooks';

import { IContentForm } from './interfaces';
import * as styled from './styled';
import { toForm } from './utils';

export interface IContentClipFormProps {
  /** The content currently being viewed. */
  content: IContentForm;
  /** A way to update the content when attaching a file. */
  setContent: (content: IContentForm) => void;
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
  setClipErrors,
}) => {
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const [, contentApi] = useContent();
  const navigate = useNavigate();
  const query = useQuery();

  const [locationId, setLocationId] = React.useState(parseInt(query.get('locationId') ?? '1')); // TODO: Should not hardcode data location.
  const [path, setPath] = React.useState(query.get('path') ?? '');

  const onAttach = async (item: IFileItem) => {
    if (values.id === 0) {
      // Add a reference to the file so that it can be copied to the API when the content is saved.
      setFieldValue('fileReferences', [
        {
          contentType: item.mimeType,
          fileName: item.name,
          path: `${item.path}/${item.name}`,
          size: item.size,
          isUploaded: false,
        } as IFileReferenceModel,
      ]);
    } else {
      await contentApi
        .attach(content.id, item.locationId, `${item.path}/${item.name}`)
        .then((data) => {
          setContent(toForm(data));
          toast.success('Attachment added to this snippet.');
        });
    }
  };

  return (
    <styled.ContentClipForm>
      <FileManager
        locationId={locationId}
        path={path}
        onAttach={onAttach}
        onNavigate={(locationId, path) => {
          setLocationId(locationId);
          setPath(path);
          navigate(`${window.location.pathname}?path=${path}`);
        }}
        setClipErrors={setClipErrors}
      />
    </styled.ContentClipForm>
  );
};
