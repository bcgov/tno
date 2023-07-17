import { Wysiwyg } from 'components/wysiwyg';
import { useFormikContext } from 'formik';
import React from 'react';
import { useContent } from 'store/hooks';
import { ContentTypeName, IFileReferenceModel } from 'tno-core';

import { IFile, Upload } from '.';
import { IContentForm } from './interfaces';
import * as styled from './styled';

export interface IMediaSummaryProps {
  file?: IFile;
  fileReference?: IFileReferenceModel;
  setStream: (stream: any) => void;
  stream?: { url: string };
  contentType?: ContentTypeName;
  setShowExpandModal?: (show: boolean) => void;
  isSummaryRequired: boolean;
}

/**
 * Provides a way to view/edit images/snippets and the summary.
 * @returns the MediaSummary
 */
export const MediaSummary: React.FC<IMediaSummaryProps> = ({
  file,
  fileReference,
  setStream,
  stream,
  contentType,
  setShowExpandModal,
  isSummaryRequired,
}) => {
  const { setFieldValue, values } = useFormikContext<IContentForm>();
  const [, { download }] = useContent();

  return (
    <styled.MediaSummary>
      <Wysiwyg
        className="summary"
        label="Summary"
        required={isSummaryRequired}
        fieldName="summary"
        expandModal={setShowExpandModal}
      />
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
    </styled.MediaSummary>
  );
};
