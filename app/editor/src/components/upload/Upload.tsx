import { Modal } from 'components/modal';
import { useModal } from 'hooks/modal';
import React, { InputHTMLAttributes, useState } from 'react';
import { Button, ButtonVariant, Col, Row } from 'tno-core';

import * as styled from './styled';

/**
 * Upload component provides a way to upload files and captures the appropriate meta-data. WIP.
 * @returns Upload component.
 */
export const Upload: React.FC<InputHTMLAttributes<HTMLButtonElement>> = ({
  type = 'button',
  className,
  children,
  ...rest
}) => {
  const [fileName, setFileName] = useState('');
  /** Duration / metadata WIP */
  // const [duration, setDuration] = useState(0);
  var reader = new FileReader();

  const { isShowing, toggle } = useModal();
  return (
    <styled.Upload>
      <Row>
        <div className="upl">
          <label htmlFor="work">Attach a File</label>
          <input
            type={'file'}
            id="work"
            hidden
            onChange={(e: any) => {
              setFileName(e.target.files[0].name);
              reader.readAsDataURL(e.target.files[0]);
              var media = new Audio(reader.result as any);
              media.onloadedmetadata = function () {
                // setDuration(media.duration);
              };
            }}
          />
        </div>
        <Col>
          {!!fileName && fileName}
          {!!fileName && (
            <div className="msg">
              <img
                style={{ marginRight: '0.2em' }}
                alt="back"
                src={process.env.PUBLIC_URL + '/assets/check_mark.svg'}
              />
              File attached
            </div>
          )}
        </Col>
        <Button className="remove" variant={ButtonVariant.danger} onClick={toggle}>
          Remove File
        </Button>
      </Row>
      {/* Modal to appear when removing a file */}
      <Modal
        isShowing={isShowing}
        hide={toggle}
        type="delete"
        headerText="Confirm Deletion"
        body="Are you sure you want to delete this file?"
        confirmText="Yes, Delete It"
        onConfirm={() => {
          setFileName('');
          toggle();
        }}
      />
    </styled.Upload>
  );
};
