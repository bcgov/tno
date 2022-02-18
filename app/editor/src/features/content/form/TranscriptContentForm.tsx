import { Button, ButtonVariant, Col, FormikTextArea, Row } from 'components';
import { useFormikContext } from 'formik';
import { IContentApi } from 'hooks';

import { IContentSubForms } from '.';

export const TranscriptContentForm: React.FC<IContentSubForms> = ({ content, setContent }) => {
  const { values } = useFormikContext<IContentApi>();
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
