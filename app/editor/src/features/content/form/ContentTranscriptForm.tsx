import React from 'react';
import { useLookup } from 'store/hooks';
import { Button, ButtonVariant, FormikContentWysiwyg, Modal, useWindowSize } from 'tno-core';

import * as styled from './styled';

/**
 * The component to be displayed when the transcript tab is selected from the content form.
 * @returns the ContentTranscriptForm
 */
export const ContentTranscriptForm: React.FC = () => {
  const [showExpandModal, setShowExpandModal] = React.useState(false);
  const onCloseOrHide = () => setShowExpandModal(!showExpandModal);
  const { height } = useWindowSize();
  const [{ tags }] = useLookup();

  return (
    <styled.ContentTranscriptForm>
      <FormikContentWysiwyg
        className="quill-body"
        name="body"
        expandModal={setShowExpandModal}
        tags={tags}
      />
      <Modal
        body={
          <FormikContentWysiwyg className="modal-quill" name="body" height={height} tags={tags} />
        }
        isShowing={showExpandModal}
        hide={onCloseOrHide}
        className="modal-full"
        customButtons={
          <Button variant={ButtonVariant.secondary} onClick={onCloseOrHide}>
            Close
          </Button>
        }
      />
    </styled.ContentTranscriptForm>
  );
};
