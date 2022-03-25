import { Button, ButtonVariant } from 'components/button';
import { Col } from 'components/flex/col';
import { Row } from 'components/flex/row';
import { FormikTextArea } from 'components/formik';
import { useFormikContext } from 'formik';

import { IContentSubForms } from '.';
import { IContentForm } from './interfaces';

export const TranscriptContentForm: React.FC<IContentSubForms> = ({ content, setContent }) => {
  const { values } = useFormikContext<IContentForm>();
  return (
    <Col style={{ margin: '3%' }}>
      <Row>
        <FormikTextArea
          name="transcription"
          label="Transcript"
          value={values.transcription}
          onChange={(e: any) => setContent({ ...content, transcription: e.target.value })}
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
