import { useFormikContext } from 'formik';
import React from 'react';
import { useContent } from 'store/hooks';
import { Col, ContentTypeName, IFileReferenceModel, Row, Show } from 'tno-core';

import { IFile, Tags, ToningGroup, Upload, Wysiwyg } from '.';
import { IContentForm } from './interfaces';
import * as styled from './styled';

export interface IMediaSummaryProps {
  file?: IFile;
  fileReference?: IFileReferenceModel;
  setStream: (stream: any) => void;
  stream: any;
  contentType?: ContentTypeName;
  setShowExpandModal?: (show: boolean) => void;
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
}) => {
  const videoRef = React.useRef<HTMLVideoElement>(null);
  const { setFieldValue, values } = useFormikContext<IContentForm>();
  const [, { download }] = useContent();

  return (
    <styled.MediaSummary>
      <Col className="media">
        <Upload
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
            if (!!videoRef.current) {
              videoRef.current.src = '';
            }
          }}
        />
      </Col>
      <Col className="summary">
        <Wysiwyg label="Summary" required fieldName="summary" expandModal={setShowExpandModal} />
        <Show visible={contentType !== ContentTypeName.Image}>
          <Row wrap="nowrap">
            <Tags />
            <ToningGroup fieldName="tonePools" />
          </Row>
        </Show>
      </Col>
    </styled.MediaSummary>
  );
};
