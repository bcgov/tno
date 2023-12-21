import 'react-quill/dist/quill.snow.css';

import { useFormikContext } from 'formik';
import React from 'react';
import { useLookup } from 'store/hooks';
import {
  Button,
  ButtonVariant,
  ContentTypeName,
  FormikContentWysiwyg,
  Modal,
  Show,
  useWindowSize,
} from 'tno-core';

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
  const [{ tags }] = useLookup();

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
        <FormikContentWysiwyg
          className="content-body"
          label="Story"
          name="body"
          expandModal={setShowExpandModal}
          tags={tags}
        />
      </Show>
      <Modal
        body={
          <FormikContentWysiwyg
            className="modal-quill"
            label={
              contentType === ContentTypeName.PrintContent ||
              contentType === ContentTypeName.Internet
                ? 'Story'
                : 'Summary'
            }
            required={summaryRequired}
            height={height}
            name={
              contentType === ContentTypeName.PrintContent ||
              contentType === ContentTypeName.Internet
                ? 'body'
                : 'summary'
            }
            tags={tags}
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
