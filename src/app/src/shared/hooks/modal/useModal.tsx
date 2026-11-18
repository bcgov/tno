import React from 'react';

import { IModalProps, Modal } from '../../components';

/**
 * Hook that provides common properties and functions to control a modal.
 * @returns Hook properties to control a modal.
 */
export const useModal = (props?: IModalProps) => {
  const [modalProps, setModalProps] = React.useState<IModalProps>(props ?? {});

  /**
   * Make the current modal show/hide based on its current state.
   */
  const toggle = modalProps.hide
    ? modalProps.hide
    : () => {
        setModalProps((state) => ({ ...state, isShowing: !state.isShowing }));
      };

  /**
   * Updates the properties of the modal and makes it visible.
   * @param props Modal properties.
   */
  const show = (props: IModalProps) => {
    setModalProps({ ...props, isShowing: true });
  };

  return {
    isShowing: modalProps.isShowing,
    toggle,
    show,
    Modal: <Modal {...modalProps} hide={toggle} />,
  };
};
