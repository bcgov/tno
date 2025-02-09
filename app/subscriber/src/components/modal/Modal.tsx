import { Button } from 'components/button';
import React from 'react';
import ReactDOM from 'react-dom';
import { FaRegQuestionCircle } from 'react-icons/fa';
import { MdClose } from 'react-icons/md';
import { Col, Row } from 'tno-core/dist/components/flex';

import * as styled from './styled';

export interface IModalProps {
  /** boolean value used to determine whether to show the modal or not */
  isShowing?: boolean;
  /** the text to show along with the cancel button to close the modal */
  cancelText?: string;
  /** the text to show along with the confirm button on the modal */
  confirmText?: string;
  /** the text for the header */
  headerText?: string;
  /** pass custom buttons directly to the modal */
  customButtons?: React.ReactNode;
  /** preset stylings for the modal */
  type?: 'delete' | 'default' | 'custom';
  hasHeight?: boolean;
  /** Form is submitting and buttons should be disabled. */
  isSubmitting?: boolean;
  /** Checking condition for enabling Confirm button*/
  enableConfirm?: boolean;
  /** the text / fragment for the body of the modal */
  body?: React.ReactNode;
  /** You can use this instead of the body */
  children?: React.ReactNode;
  /** function used to toggle the modal visibility */
  onClose?: () => void;
  /** function to be called when user clicks continue/confirm button */
  onConfirm?: () => void;
}

/**
 *  Modal used to display informative and confirmation messages through the application.
 *  Portal is used to allow the modal to be appended outside of inner elements.
 **/
export const Modal: React.FC<IModalProps> = ({
  isShowing,
  cancelText,
  confirmText,
  headerText,
  body,
  customButtons,
  type,
  hasHeight,
  enableConfirm = true,
  isSubmitting: initIsSubmitting,
  children,
  onConfirm,
  onClose,
}) => {
  const [isSubmitting, setIsSubmitting] = React.useState(initIsSubmitting);

  React.useEffect(() => {
    setIsSubmitting(initIsSubmitting);
  }, [initIsSubmitting]);

  return isShowing
    ? ReactDOM.createPortal(
        <styled.Modal hasHeight={hasHeight}>
          <div className="modal-overlay">
            <div className="modal-wrapper" aria-modal aria-hidden tabIndex={-1} role="dialog">
              <div className="modal">
                {!!headerText && (
                  <Row className="modal-header">
                    <span>
                      <FaRegQuestionCircle />
                    </span>
                    <h1>{headerText}</h1>
                    <span onClick={onClose} style={{ cursor: 'pointer' }}>
                      <MdClose />
                    </span>
                  </Row>
                )}
                <Col alignItems="flex-start" className="modal-body">
                  {typeof body === 'string' ? (
                    <div dangerouslySetInnerHTML={{ __html: body as any }}></div>
                  ) : (
                    body ?? children
                  )}
                </Col>
                <Row className="button-row">
                  {!customButtons ? (
                    <>
                      <Button variant={'secondary'} onClick={onClose} disabled={isSubmitting}>
                        {cancelText ?? 'Cancel'}
                      </Button>
                      <Button
                        variant={type === 'delete' ? 'error' : 'primary'}
                        onClick={onConfirm}
                        disabled={isSubmitting || !enableConfirm}
                      >
                        {confirmText ?? 'Continue'}
                      </Button>
                    </>
                  ) : (
                    customButtons
                  )}
                </Row>
              </div>
            </div>
          </div>
        </styled.Modal>,
        document.body,
      )
    : null;
};
