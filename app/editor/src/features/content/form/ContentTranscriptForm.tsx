import React from 'react';
import { useLookup } from 'store/hooks';
import { Button, ButtonVariant, FormikContentWysiwyg, Modal, useWindowSize } from 'tno-core';

import { useExtractTags } from './hooks';
import * as styled from './styled';

export interface IContentTranscriptFormProps {
  setParsedTags?: React.Dispatch<React.SetStateAction<string[]>>;
}

/**
 * The component to be displayed when the transcript tab is selected from the content form.
 * @returns the ContentTranscriptForm
 */
export const ContentTranscriptForm: React.FC<IContentTranscriptFormProps> = ({ setParsedTags }) => {
  const [showExpandModal, setShowExpandModal] = React.useState(false);
  const onCloseOrHide = () => setShowExpandModal(!showExpandModal);
  const { height } = useWindowSize();
  const [{ tags }] = useLookup();
  const getTags = useExtractTags({ setParsedTags });

  return (
    <styled.ContentTranscriptForm>
      <FormikContentWysiwyg
        className="content-body"
        name="body"
        expandModal={setShowExpandModal}
        tags={tags}
        onChange={(text) => getTags('body', text)}
      />
      <Modal
        body={
          <FormikContentWysiwyg
            className="modal-quill"
            name="body"
            height={height}
            tags={tags}
            onChange={(text) => getTags('body', text)}
          />
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
