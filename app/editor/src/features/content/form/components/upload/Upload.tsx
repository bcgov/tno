import { IContentForm } from 'features/content/form/interfaces';
import { isWorkOrderStatus } from 'features/content/utils';
import { useFormikContext } from 'formik';
import React, { ButtonHTMLAttributes } from 'react';
import { FileUploader } from 'react-drag-drop-files';
import { FaDownload, FaTrash, FaUpload } from 'react-icons/fa';
import { FaMobile } from 'react-icons/fa6';
import { toast } from 'react-toastify';
import { useWorkOrders } from 'store/hooks';
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
  WorkOrderStatusName,
  WorkOrderTypeName,
} from 'tno-core';

import { toModel } from '../../utils';
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
  const [, { ffmpeg }] = useWorkOrders();
  const { isShowing: showFFmpegModal, toggle: toggleFFmpeg } = useModal();

  const [file, setFile] = React.useState<IFile>();

  const [fileName, setFileName] = React.useState<string>();
  const [fileBlobUrl, setFileBlobUrl] = React.useState<string>();
  const fileReference = values.fileReferences.length ? values.fileReferences[0] : undefined;
  const processing = values.workOrders.some(
    (wo) =>
      wo.workType === WorkOrderTypeName.FFmpeg && wo.status === WorkOrderStatusName.InProgress,
  );

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

  const handleFFmpeg = React.useCallback(
    async (values: IContentForm) => {
      try {
        // Save before submitting request.
        const response = await ffmpeg(toModel(values));

        if (response.status === 200) toast.success('A FFmpeg process has been requested');
        else if (response.status === 208) {
          if (response.data.status === WorkOrderStatusName.Completed)
            toast.warn('Content has already been processed by FFmpeg');
          else toast.warn(`An active request for FFmpeg already exists`);
        }
      } catch {
        // Ignore this failure it is handled by our global ajax requests.
      }
    },
    [ffmpeg],
  );

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
            <audio controls>
              <source src={`${stream?.url}`} type={`${fileReference?.contentType}`} />
              HTML5 Audio is required
            </audio>
          </Col>
        </Show>
        <Show visible={fileReference?.contentType.startsWith('video/')}>
          <Col flex="1">
            <video controls preload="metadata">
              <source src={`${stream?.url}`} type={`${fileReference?.contentType}`} />
              HTML5 Video is required
            </video>
          </Col>
        </Show>
      </Show>
      <Show visible={!!file || !!fileName || !!fileReference}>
        <Row justifyContent="flex-end" gap="0.5rem" alignItems="center">
          <span className="file-name">{fileName}</span>

          <Row gap="0.25rem">
            <Show visible={values.contentType === ContentTypeName.AudioVideo}>
              <Button
                variant={ButtonVariant.link}
                onClick={() =>
                  isWorkOrderStatus(values.workOrders, WorkOrderTypeName.FFmpeg, [
                    WorkOrderStatusName.Completed,
                  ])
                    ? toggleFFmpeg()
                    : handleFFmpeg(values)
                }
                disabled={
                  !onDownload || !file || !downloadable || !fileName || !fileReference || processing
                }
                className="file-name"
                tooltip="Process file"
              >
                {processing ? <Spinner size="8px" /> : <FaMobile />}
              </Button>
            </Show>
          </Row>
        </Row>
        <Row className="upload-buttons">
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
      <Modal
        headerText="Confirm FFmpeg Request"
        body="Content has already been processed by FFmpeg, do you want to process again?"
        isShowing={showFFmpegModal}
        hide={toggleFFmpeg}
        type="default"
        confirmText="Yes, process"
        onConfirm={async () => {
          try {
            await handleFFmpeg(values);
          } finally {
            toggleFFmpeg();
          }
        }}
      />
    </styled.Upload>
  );
};
