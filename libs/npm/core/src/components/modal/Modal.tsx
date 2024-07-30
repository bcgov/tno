import React from 'react';
import ReactDOM from 'react-dom';

import { Button, ButtonVariant } from '../button';
import { Row } from '../flex';
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
  /** a component / fragment for the body of the modal */
  component?: React.ReactNode;
  /** pass custom buttons directly to the modal */
  customButtons?: React.ReactNode;
  /** preset stylings for the modal */
  type?: 'delete' | 'default' | 'custom';
  hasHeight?: boolean;
  className?: 'modal-full';
  /** boolean value used to determine whether to enabled the confirm button or not */
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
  component,
  customButtons,
  type,
  hasHeight,
  className,
  enableConfirm = true,
}) => {
  return isShowing
    ? ReactDOM.createPortal(
        <styled.Modal hasHeight={hasHeight}>
          <div className="modal-overlay">
            <div className="modal-wrapper" aria-modal aria-hidden tabIndex={-1} role="dialog">
              <div className={className ?? 'modal-popup'}>
                {!!headerText && (
                  <Row className="modal-header">
                    <h1>{headerText}</h1>
                  </Row>
                )}
                <Row className="modal-body">
                  {body && <div dangerouslySetInnerHTML={{ __html: body }}></div>}
                </Row>
                {!!component && <Row className="modal-body">{component}</Row>}
                {type !== 'custom' && (
                  <Row className="button-row">
                    <Button
                      disabled={!enableConfirm}
                      variant={type === 'delete' ? ButtonVariant.danger : ButtonVariant.secondary}
                      onClick={onConfirm}
                    >
                      {confirmText ?? 'Continue'}
                    </Button>
                    <Button
                      variant={type === 'delete' ? ButtonVariant.secondary : ButtonVariant.danger}
                      onClick={hide}
                    >
                      {cancelText ?? 'Cancel'}
                    </Button>
                  </Row>
                )}
                {!!customButtons && <Row className="button-row"> {customButtons} </Row>}
              </div>
            </div>
          </div>
        </styled.Modal>,
        document.body,
      )
    : null;
};
