import { IContentForm } from 'features/content/form/interfaces';
import { useFormikContext } from 'formik';
import React, { ButtonHTMLAttributes } from 'react';
import { FileUploader } from 'react-drag-drop-files';
import { FaDownload, FaTrash, FaUpload } from 'react-icons/fa';
import {
  Button,
  ButtonVariant,
  Col,
  ContentTypeName,
  Modal,
  Row,
  Show,
  Spinner,
  useModal,
} from 'tno-core';

import { useContentForm } from '../../hooks';
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
  const { isProcessing } = useContentForm(values);

  const fileRef = React.useRef<HTMLVideoElement>(null);

  const [wasProcessing, setWasProcessing] = React.useState(isProcessing);
  const [file, setFile] = React.useState<IFile>();
  const [fileName, setFileName] = React.useState<string>();
  const [fileBlobUrl, setFileBlobUrl] = React.useState<string>();
  const fileReference = values.fileReferences.length ? values.fileReferences[0] : undefined;

  React.useEffect(() => {
    const newFileName = generateName(initFile);
    if (!!initFile && newFileName !== fileName) {
      setFile(initFile);
      setFileName(newFileName);
    }
    // Only update when initFile changes
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [initFile]);

  React.useEffect(() => {
    if (!stream) {
      // When the stream is removed it needs to update the state in this component.
      // Worried this may result in an odd race condition with 'file' and 'stream'.
      setFile(undefined);
      setFileName(undefined);
    }
  }, [stream]);

  React.useEffect(() => {
    // When the processing completes it needs to load the file again.
    if (wasProcessing && !isProcessing && fileRef.current) {
      fileRef.current.load();
    }
    setWasProcessing(isProcessing);
    // Only interested in when the processing changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [isProcessing]);

  React.useEffect(() => {
    const updatedFileName = generateName(file);
    setFileName(updatedFileName);
    async function updateFileBlobUrl() {
      const buffer = await file?.arrayBuffer?.();
      if (buffer) {
        const newBlob = new Blob([buffer], { type: file?.type });
        setFileBlobUrl(URL.createObjectURL(newBlob));
      }
    }
    updateFileBlobUrl();
  }, [file]);

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
      <Show visible={!fileName || (!fileReference && !file)}>
        <Col className="drop-box" flex="1">
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
        </Col>
      </Show>
      <Show
        visible={
          (!!stream && contentType === ContentTypeName.Image) ||
          (!!file && !!fileBlobUrl && file.type?.includes('image'))
        }
      >
        <Col flex="1">
          <img
            height="350"
            width="400"
            alt="media"
            className="image"
            src={stream?.url || fileBlobUrl}
          ></img>
        </Col>
      </Show>
      <Show visible={!!stream && contentType === ContentTypeName.AudioVideo && !!fileReference}>
        <Show visible={fileReference?.contentType.startsWith('audio/')}>
          <Col flex="1">
            <audio controls ref={fileRef}>
              <source src={`${stream?.url}`} type={`${fileReference?.contentType}`} />
              HTML5 Audio is required
            </audio>
          </Col>
        </Show>
        <Show visible={fileReference?.contentType.startsWith('video/')}>
          <Col flex="1">
            <video controls preload="metadata" ref={fileRef}>
              <source src={`${stream?.url}`} type={`${fileReference?.contentType}`} />
              HTML5 Video is required
            </video>
          </Col>
        </Show>
      </Show>
      <Show visible={!!file || !!fileName || !!fileReference}>
        <Row justifyContent="flex-end" gap="0.5rem" alignItems="center">
          <span className="file-name">{fileName}</span>
        </Row>
        <Row className="upload-buttons">
          {isProcessing && <Spinner title="Processing" />}
          <Button
            variant={ButtonVariant.link}
            onClick={() => onDownload?.()}
            disabled={!onDownload || !file || !downloadable || !fileName || !fileReference}
            className="file-name"
            tooltip={`Download ${!!fileName ? fileName : 'not available'}`}
          >
            <FaDownload />
          </Button>
          <Button
            disabled={!fileName || (!fileReference && !file)}
            variant={ButtonVariant.danger}
            className="delete"
            onClick={() => {
              if (verifyDelete) toggle();
              else handleDelete();
            }}
          >
            <FaTrash />
          </Button>
        </Row>
      </Show>
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
