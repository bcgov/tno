import React from 'react';
import { useParams, useSearchParams } from 'react-router-dom';
import { useWorkOrders } from 'store/hooks';
import { Col, Row } from 'tno-core';

import * as styled from './styled';

/**
 * Component to submit access requests.
 * @returns Access request page.
 */
export const TranscriptionRequest: React.FC = () => {
  const { id } = useParams();
  const [params] = useSearchParams();
  const [requested, setRequested] = React.useState(false);
  const uid = params.get('uid');
  const [, { transcribeAnonymous }] = useWorkOrders();

  const handleTranscribe = React.useCallback(
    async (contentId: number, uid: number) => {
      try {
        const response: any = await transcribeAnonymous(contentId, uid);

        if (response.transcriptionStatus === 'Approved') {
          setRequested(true);
        } else {
          setRequested(false);
        }
      } catch {
        // Ignore this failure it is handled by our global ajax requests.
      }
    },
    [transcribeAnonymous],
  );

  React.useEffect(() => {
    if (id && uid) {
      handleTranscribe(parseInt(id), parseInt(uid));
    }
  }, [handleTranscribe, id, uid]);

  return (
    <styled.TranscriptionRequest>
      <Col className="containing-col">
        <img alt="MMI Logo" className="app-logo" src="/assets/MMinsights_logo_dark_text.svg" />
        <Row className="containing-row">
          <Col className="main-box">
            <hr />
            <p className="top-bar-box">
              {requested
                ? 'The transcript has already been requested.'
                : 'The transcript request has been submitted.'}
            </p>
            <div className={'containing-box'}>
              <Col className="message-box">
                You will receive an email to let you know when the transcript has been prepared and
                is ready to view.
              </Col>
            </div>
          </Col>
        </Row>
      </Col>
    </styled.TranscriptionRequest>
  );
};
