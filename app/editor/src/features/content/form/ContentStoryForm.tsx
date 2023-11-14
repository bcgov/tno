import 'react-quill/dist/quill.snow.css';

import { Wysiwyg } from 'components/wysiwyg';
import { useFormikContext } from 'formik';
import React from 'react';
import { Button, ButtonVariant, ContentTypeName, Modal, Show, useWindowSize } from 'tno-core';

import { IContentForm } from './interfaces';
import { MediaSummary } from './MediaSummary';
import * as styled from './styled';
import { isSummaryRequired } from './utils';

export interface IContentStoryFormProps {
  contentType: ContentTypeName;
  summaryRequired?: boolean;
}

/**
 * ContentStoryForm component provides a form for content summary details.
 * @param param0 Component properties
 * @returns A new instance of a component.
 */
export const ContentStoryForm: React.FC<IContentStoryFormProps> = ({
  contentType,
  summaryRequired: initSummaryRequired,
}) => {
  const { values } = useFormikContext<IContentForm>();
  const [showExpandModal, setShowExpandModal] = React.useState(false);
  const { height } = useWindowSize();

  const [summaryRequired, setSummaryRequired] = React.useState(
    initSummaryRequired ?? isSummaryRequired(values),
  );

  React.useEffect(() => {
    setSummaryRequired(isSummaryRequired(values));
    // Only interested in changing this value when the media type changes.
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [values.mediaTypeId]);

  return (
    <styled.ContentStoryForm className="content-properties">
      <Show
        visible={
          contentType === ContentTypeName.Image || contentType === ContentTypeName.AudioVideo
        }
      >
        <MediaSummary setShowExpandModal={setShowExpandModal} isSummaryRequired={summaryRequired} />
      </Show>
      <Show
        visible={
          contentType !== ContentTypeName.AudioVideo && contentType !== ContentTypeName.Image
        }
      >
        <Wysiwyg
          className="content-body"
          label="Story"
          fieldName="body"
          expandModal={setShowExpandModal}
        />
      </Show>
      <Modal
        body={
          <Wysiwyg
            className="modal-quill"
            label={
              contentType === ContentTypeName.PrintContent || contentType === ContentTypeName.Story
                ? 'Story'
                : 'Summary'
            }
            required={summaryRequired}
            height={height}
            fieldName={
              contentType === ContentTypeName.PrintContent || contentType === ContentTypeName.Story
                ? 'body'
                : 'summary'
            }
          />
        }
        isShowing={showExpandModal}
        hide={() => setShowExpandModal(!showExpandModal)}
        className="modal-full"
        customButtons={
          <Button
            variant={ButtonVariant.secondary}
            onClick={() => setShowExpandModal(!showExpandModal)}
          >
            Close
          </Button>
        }
      />
    </styled.ContentStoryForm>
  );
};
