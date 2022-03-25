import { Button, ButtonVariant } from 'components/button';
import { Col } from 'components/col';
import { FormikTextArea } from 'components/formik';
import { Row } from 'components/row';
import { useFormikContext } from 'formik';

import { IContentForm } from './interfaces';

/**
 * The component to be displayed when the transcript tab is selected from the content form.
 * @returns the TranscriptContentForm
 */
export const TranscriptContentForm: React.FC = () => {
  const { values, setFieldValue } = useFormikContext<IContentForm>();
  return (
    <Col style={{ margin: '3%' }}>
      <Row>
        <FormikTextArea
          name="transcription"
          label="Transcript"
          value={values.transcription}
          onChange={(e: any) => setFieldValue('transcription', e.target.value)}
          style={{ width: '1000px', height: '500px' }}
        />
      </Row>
      <Row>
        <Button disabled variant={ButtonVariant.action}>
          Auto Transcribe
        </Button>
      </Row>
    </Col>
  );
};
