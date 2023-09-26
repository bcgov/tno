import { Wysiwyg } from 'components/wysiwyg';
import { useFormikContext } from 'formik';
import React from 'react';
import { useContent } from 'store/hooks';
import { ContentTypeName, IFileReferenceModel, Row } from 'tno-core';

import { IFile, Upload } from './components';
import { IContentForm } from './interfaces';
import * as styled from './styled';

export interface IContentTranscriptFormProps {
  file?: IFile;
  fileReference?: IFileReferenceModel;
  setStream: (stream: any) => void;
  stream?: { url: string };
  contentType?: ContentTypeName;
  setShowExpandModal?: (show: boolean) => void;
}

/**
 * The component to be displayed when the transcript tab is selected from the content form.
 * @returns the ContentTranscriptForm
 */
export const ContentTranscriptForm: React.FC<IContentTranscriptFormProps> = ({
  file,
  fileReference,
  setStream,
  stream,
  contentType,
  setShowExpandModal,
}) => {
  const [, { download }] = useContent();
  const { setFieldValue, values } = useFormikContext<IContentForm>();

  return (
    <styled.ContentTranscriptForm>
      <Row>
        <Wysiwyg fieldName="body" expandModal={setShowExpandModal} className="summary" />
        <Upload
          className="media"
          contentType={contentType}
          id="upload"
          name="file"
          file={file}
          stream={stream}
          downloadable={fileReference?.isUploaded}
          onSelect={(e) => {
            const file = (e as IFile).name ? (e as IFile) : undefined;
            setFieldValue('file', file);
            // Remove file reference.
            setFieldValue('fileReferences', []);
          }}
          onDownload={() => {
            download(values.id, file?.name ?? `${values.otherSource}-${values.id}`);
          }}
          onDelete={() => {
            setStream(undefined);
          }}
        />
      </Row>
    </styled.ContentTranscriptForm>
  );
};
