import { FormikTextArea } from 'components/formik';
import { useFormikContext } from 'formik';

import { IContentForm } from './interfaces';
import * as styled from './styled';

/**
 * The component to be displayed when the transcript tab is selected from the content form.
 * @returns the ContentTranscriptForm
 */
export const ContentTranscriptForm: React.FC = () => {
  const { values } = useFormikContext<IContentForm>();

  return (
    <styled.ContentTranscriptForm>
      <FormikTextArea name="body" label="Transcript" value={values.body} />
    </styled.ContentTranscriptForm>
  );
};
