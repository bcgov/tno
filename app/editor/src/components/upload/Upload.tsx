import { Modal } from 'components/modal';
import { useModal } from 'hooks/modal';
import React, { ButtonHTMLAttributes } from 'react';
import { Button, ButtonVariant, Col, Show } from 'tno-core';

import { IFile } from '.';
import * as styled from './styled';
import { generateName } from './utils';

const nativeInputValueSetter = Object.getOwnPropertyDescriptor(
  window.HTMLInputElement.prototype,
  'value',
)?.set;

export interface IUploadProps
  extends Omit<ButtonHTMLAttributes<HTMLButtonElement>, 'onSelect' | 'value'> {
  onSelect?: (e: React.ChangeEvent<HTMLInputElement>) => void;
  onDownload?: () => void;
  onDelete?: () => void;
  file?: IFile;
  verifyDelete?: boolean;
  downloadable?: boolean;
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
  ...rest
}) => {
  const { isShowing, toggle } = useModal();
  const fileRef = React.useRef<HTMLInputElement>(null);

  const [file, setFile] = React.useState<IFile>();

  /** Duration / metadata WIP */
  // const [duration, setDuration] = useState(0);
  const reader = new FileReader();
  const fileName = generateName(file) ?? generateName(initFile);

  React.useEffect(() => {
    if (!!initFile) {
      setFile(undefined);
    }
  }, [initFile]);

  const handleDelete = () => {
    if (!!fileRef.current) {
      nativeInputValueSetter?.call(fileRef.current, '');
      const event = new Event('change', { bubbles: true });
      fileRef.current.dispatchEvent(event);
      onDelete?.();
    }
    setFile(undefined);
  };

  return (
    <styled.Upload className={className ?? ''}>
      <Button
        variant={ButtonVariant.secondary}
        {...rest}
        onClick={(e) => {
          onClick?.(e);
          if (!onClick) fileRef.current?.click();
        }}
      >
        Attach File
      </Button>
      <input
        id={id}
        type="file"
        name={name}
        ref={fileRef}
        hidden
        onChange={(e) => {
          onSelect?.(e);
          const files = e?.target?.files ?? [];
          if (files.length) {
            const file = files[0];
            setFile(file);
            reader.readAsDataURL(file);
            var media = new Audio(reader.result as any);
            media.onloadedmetadata = function () {
              // setDuration(media.duration);
            };
          }
        }}
      />
      <Col className="file">
        <Show visible={!!fileName}>
          <Button
            variant={ButtonVariant.link}
            onClick={() => onDownload?.()}
            disabled={!onDownload || !!file || !downloadable}
          >
            {fileName}
          </Button>
        </Show>
      </Col>
      <Show visible={!!fileName}>
        <Button
          variant={ButtonVariant.danger}
          onClick={() => {
            if (verifyDelete) toggle();
            else handleDelete();
          }}
        >
          Remove File
        </Button>
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
