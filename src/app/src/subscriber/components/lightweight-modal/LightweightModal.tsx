import React from 'react';

import * as styled from './styled';

export interface LightweightModalProps {
  open: boolean;
  close: () => void;
  children: React.ReactNode;
}

export const LightweightModal: React.FC<LightweightModalProps> = ({ children, open, close }) => {
  const ref = React.useRef<HTMLDialogElement>(null);

  React.useEffect(() => {
    if (open) {
      ref.current?.showModal();
    } else {
      ref.current?.close();
    }
  }, [open]);

  return (
    <styled.LightweightModal ref={ref}>
      {children}
      <button className="close-button" onClick={close}>
        Close
      </button>
    </styled.LightweightModal>
  );
};
