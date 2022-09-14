import { FormikTextArea } from 'components/formik';
import { useFormikContext } from 'formik';
import { useContent } from 'store/hooks';
import { Button, ButtonVariant, Show } from 'tno-core';

import { IContentForm } from './interfaces';
import * as styled from './styled';
import { toModel } from './utils';

/**
 * The component to be displayed when the transcript tab is selected from the content form.
 * @returns the ContentTranscriptForm
 */
export const ContentTranscriptForm: React.FC = () => {
  const [, { transcribe, nlp }] = useContent();
  const { values, setFieldValue, isSubmitting } = useFormikContext<IContentForm>();

  const handleTranscribe = async (values: IContentForm) => {
    await transcribe(toModel(values));
  };

  const handleNLP = async (values: IContentForm) => {
    await nlp(toModel(values));
  };

  return (
    <styled.ContentTranscriptForm>
      <Show visible={!!values.id}>
        <Button
          onClick={() => handleTranscribe(values)}
          variant={ButtonVariant.action}
          disabled={
            isSubmitting ||
            !values.fileReferences.length ||
            (values.fileReferences.length > 0 && !values.fileReferences[0].isUploaded)
          }
        >
          Transcribe
        </Button>
        <Button
          onClick={() => handleNLP(values)}
          variant={ButtonVariant.action}
          disabled={isSubmitting}
        >
          NLP
        </Button>
      </Show>
      <FormikTextArea
        name="transcription"
        label="Transcript"
        style={{ height: '290px', minHeight: '65px' }}
        value={values.transcription}
        onChange={(e: any) => setFieldValue('transcription', e.target.value)}
      />
    </styled.ContentTranscriptForm>
  );
};
