import { Button, ButtonVariant } from 'components/button';
import { Row } from 'components/flex/row';
import React from 'react';
import ReactDOM from 'react-dom';

import * as styled from './styled';

export interface IModalProps {
  /** function used to toggle the modal visibility */
  hide: () => void;
  /** boolean value used to determine whether to show the modal or not */
  isShowing: boolean;
  /** the text to show along with the cancel button to close the modal */
  cancelText?: string;
  /** the text to show along with the confirm button on the modal */
  confirmText?: string;
  /** function to be called when user clicks continue/confirm button */
  onConfirm?: () => void;
  /** the text for the header */
  headerText?: string;
  /** the text / fragment for the body of the modal */
  body?: string | React.ReactFragment;
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
}) => {
  return isShowing
    ? ReactDOM.createPortal(
        <styled.Modal>
          <div className="modal-overlay">
            <div className="modal-wrapper" aria-modal aria-hidden tabIndex={-1} role="dialog">
              <div className="modal">
                <Row className="modal-header">
                  <h1>{headerText}</h1>
                </Row>
                <Row>
                  <p>{body}</p>
                </Row>
                <Row className="button-row">
                  <Button variant={ButtonVariant.action} onClick={onConfirm}>
                    {confirmText ?? 'Continue'}
                  </Button>
                  <Button variant={ButtonVariant.danger} onClick={hide}>
                    {cancelText ?? 'Cancel'}
                  </Button>
                </Row>
              </div>
            </div>
          </div>
        </styled.Modal>,
        document.body,
      )
    : null;
};
