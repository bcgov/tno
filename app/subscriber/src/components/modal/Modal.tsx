import { Button } from 'components/button';
import React from 'react';
import ReactDOM from 'react-dom';
import { Col, Row } from 'tno-core/dist/components/flex';

import * as styled from './styled';

export interface IModalProps {
  /** function used to toggle the modal visibility */
  hide?: () => void;
  /** boolean value used to determine whether to show the modal or not */
  isShowing?: boolean;
  /** the text to show along with the cancel button to close the modal */
  cancelText?: string;
  /** the text to show along with the confirm button on the modal */
  confirmText?: string;
  /** function to be called when user clicks continue/confirm button */
  onConfirm?: () => void;
  /** the text for the header */
  headerText?: string;
  /** the text / fragment for the body of the modal */
  body?: React.ReactNode;
  /** pass custom buttons directly to the modal */
  customButtons?: React.ReactNode;
  /** preset stylings for the modal */
  type?: 'delete' | 'default' | 'custom';
  hasHeight?: boolean;
  /** Form is submitting and buttons should be disabled. */
  isSubmitting?: boolean;
  /** Checking condition for enabling Confirm button*/
  enableConfirm?: boolean;
}

/**
 *  Modal used to display informative and confirmation messages through the application.
 *  Portal is used to allow the modal to be appended outside of inner elements.
 **/
export const Modal: React.FC<IModalProps> = ({
  isShowing,
  cancelText,
  confirmText,
  onConfirm,
  headerText,
  hide,
  body,
  customButtons,
  type,
  hasHeight,
  enableConfirm = true,
  isSubmitting: initIsSubmitting,
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
                    <h1>{headerText}</h1>
                  </Row>
                )}
                <Col alignItems="flex-start" className="modal-body">
                  {body}
                </Col>
                <Row className="button-row">
                  {!customButtons ? (
                    <>
                      <Button
                        variant={type === 'delete' ? 'error' : 'primary'}
                        onClick={onConfirm}
                        disabled={isSubmitting && !enableConfirm}
                      >
                        {confirmText ?? 'Continue'}
                      </Button>
                      <Button variant={'secondary'} onClick={hide} disabled={isSubmitting}>
                        {cancelText ?? 'Cancel'}
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
