import { IContentForm } from 'features/content/form/interfaces';
import { useFormikContext } from 'formik';
import React, { ButtonHTMLAttributes } from 'react';
import { FileUploader } from 'react-drag-drop-files';
import { FaTrash, FaUpload } from 'react-icons/fa';
import { Button, ButtonVariant, Col, ContentTypeName, Modal, Row, Show, useModal } from 'tno-core';

import { IFile } from '.';
import * as styled from './styled';
import { generateName } from './utils';

export interface IUploadProps
  extends Omit<ButtonHTMLAttributes<HTMLButtonElement>, 'onSelect' | 'value'> {
  onSelect?: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onDownload?: () => void;
  onDelete?: () => void;
  file?: IFile;
  verifyDelete?: boolean;
  downloadable?: boolean;
  stream?: { url: string };
  contentType?: ContentTypeName;
}

/**
 * Upload component provides a way to upload files and captures the appropriate meta-data. WIP.
 * @returns Upload component.
 */
export const Upload: React.FC<IUploadProps> = ({
  id,
  name,
  className,
  file: initFile,
  verifyDelete = true,
  downloadable = true,
  onClick,
  onSelect,
  onDownload,
  onDelete,
  contentType,
  stream,
  ...rest
}) => {
  const { isShowing, toggle } = useModal();
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  const [file, setFile] = React.useState<IFile>();
  const fileName = generateName(file) ?? generateName(initFile);
  const fileReference = values.fileReferences.length ? values.fileReferences[0] : undefined;

  React.useEffect(() => {
    if (!!initFile) {
      setFile(undefined);
    }
  }, [initFile]);

  const handleDelete = () => {
    if (!!file || values.fileReferences.length > 0) {
      setFile(undefined);
      setFieldValue('fileReferences', []);
      setFieldValue('file', undefined);
      onDelete?.();
    }
  };

  return (
    <styled.Upload className={className ?? ''}>
      <Col>
        <div className="file-action">
          <Button
            variant={ButtonVariant.link}
            onClick={() => onDownload?.()}
            disabled={!onDownload || !!file || !downloadable || !fileName}
            className="file-name"
            tooltip={`Download ${!!fileName ? fileName : 'not available'}`}
          >
            {fileName ?? 'No file attached'}
          </Button>
          <Button
            disabled={!fileName}
            variant={ButtonVariant.danger}
            className="delete"
            onClick={() => {
              if (verifyDelete) toggle();
              else handleDelete();
            }}
          >
            <FaTrash className="indicator" /> Remove file
          </Button>
        </div>

        <Show visible={!fileName}>
          <Row className="drop-box">
            <FileUploader
              children={
                <div className="upload-box">
                  <Col className="body">
                    <FaUpload className="upload-image" />
                    <Row className="text">Drag and drop file here</Row>
                    <Row className="text">OR</Row>
                    <Row className="text">
                      <Button>Browse Files</Button>
                    </Row>
                  </Col>
                </div>
              }
              handleChange={(e: any) => {
                onSelect?.(e);
                const file = e as IFile;
                if (!!file) {
                  setFile(file);
                }
              }}
            />
          </Row>
        </Show>
        <Show visible={!!stream && contentType === ContentTypeName.Image}>
          <Col>
            <img height="300" width="500" alt="" className="object-fit" src={stream?.url}></img>
          </Col>
        </Show>

        <Show visible={!!stream && contentType === ContentTypeName.AudioVideo && !!fileReference}>
          <Show visible={fileReference?.contentType.startsWith('audio/')}>
            <audio src={stream?.url} controls>
              HTML5 Audio is required
            </audio>
          </Show>
          <Show visible={fileReference?.contentType.startsWith('video/')}>
            <video src={stream?.url} controls>
              HTML5 Video is required
            </video>
          </Show>
        </Show>
      </Col>
      {/* Modal to appear when removing a file */}
      <Modal
        isShowing={isShowing}
        hide={toggle}
        type="delete"
        headerText="Confirm Removal"
        body="Are you sure you want to remove this file?  You will still need to save before it is deleted."
        confirmText="Yes, Remove It"
        onConfirm={() => {
          handleDelete();
          toggle();
        }}
      />
    </styled.Upload>
  );
};
