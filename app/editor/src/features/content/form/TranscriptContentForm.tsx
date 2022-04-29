import { FormikTextArea } from 'components/formik';
import { useFormikContext } from 'formik';

import { IContentForm } from './interfaces';
import * as styled from './styled';

/**
 * The component to be displayed when the transcript tab is selected from the content form.
 * @returns the TranscriptContentForm
 */
export const TranscriptContentForm: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  return (
    <styled.TranscriptContentForm>
      <FormikTextArea
        name="transcription"
        label="Transcript"
        value={values.transcription}
        onChange={(e: any) => setFieldValue('transcription', e.target.value)}
        style={{ width: '1000px', height: '500px' }}
      />
    </styled.TranscriptContentForm>
  );
};
