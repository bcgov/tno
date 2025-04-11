import 'react-quill-new/dist/quill.snow.css';

import { useFormikContext } from 'formik';
import React from 'react';
import { useLookup } from 'store/hooks';
import { ContentTypeName, FormikContentWysiwyg, Show } from 'tno-core';

import { useExtractTags } from './hooks';
import { IContentForm } from './interfaces';
import { MediaSummary } from './MediaSummary';
import * as styled from './styled';
import { isSummaryRequired } from './utils';

export interface IContentStoryFormProps {
  contentType: ContentTypeName;
  summaryRequired?: boolean;
  setParsedTags?: React.Dispatch<React.SetStateAction<string[]>>;
}

/**
 * ContentStoryForm component provides a form for content summary details.
 * @param param0 Component properties
 * @returns A new instance of a component.
 */
export const ContentStoryForm: React.FC<IContentStoryFormProps> = ({
  // Component properties
  contentType,
  summaryRequired: initSummaryRequired,
  setParsedTags,
}) => {
  const { values } = useFormikContext<IContentForm>();
  const [{ tags }] = useLookup();
  const getTags = useExtractTags({ setParsedTags });

  // Determine if content type is body (Story) or summary based content
  const isBodyContent =
    contentType === ContentTypeName.PrintContent || contentType === ContentTypeName.Internet;

  // Field name based on content type
  const fieldName = isBodyContent ? 'body' : 'summary';

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
        <MediaSummary isSummaryRequired={summaryRequired} getTags={getTags} />
      </Show>
      <Show
        visible={
          contentType !== ContentTypeName.AudioVideo && contentType !== ContentTypeName.Image
        }
      >
        <FormikContentWysiwyg
          className="content-body"
          label={isBodyContent ? 'Story' : 'Summary'}
          name={fieldName}
          tags={tags}
          onChange={(text) => getTags(fieldName, text)}
        />
      </Show>
    </styled.ContentStoryForm>
  );
};
