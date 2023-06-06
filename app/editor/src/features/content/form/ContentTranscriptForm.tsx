import { Wysiwyg } from 'components/wysiwyg';
import React from 'react';
import { Button, ButtonVariant, Modal } from 'tno-core';

import * as styled from './styled';

/**
 * The component to be displayed when the transcript tab is selected from the content form.
 * @returns the ContentTranscriptForm
 */
export const ContentTranscriptForm: React.FC = () => {
  const [showExpandModal, setShowExpandModal] = React.useState(false);
  const onCloseOrHide = () => setShowExpandModal(!showExpandModal);
  return (
    <styled.ContentTranscriptForm>
      <Wysiwyg fieldName="body" expandModal={setShowExpandModal} />
      <Modal
        body={<Wysiwyg className="modal-quill" fieldName="body" />}
        isShowing={showExpandModal}
        hide={onCloseOrHide}
        customButtons={
          <Button variant={ButtonVariant.secondary} onClick={onCloseOrHide}>
            Close
          </Button>
        }
      />
    </styled.ContentTranscriptForm>
  );
};
