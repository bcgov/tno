import { IContentForm } from 'features/content/form/interfaces';
import React from 'react';
import { FaExternalLinkAlt } from 'react-icons/fa';
import { Col, Row } from 'tno-core';

import * as styled from './styled';

export interface IPublishedSectionProps {
  /** Form values */
  values: IContentForm;
}

export const PublishedSection: React.FC<IPublishedSectionProps> = ({ values }) => {
  const [subscriberLink, setSubscriberLink] = React.useState<string>('');
  React.useEffect(() => {
    fetch('/links.json')
      .then((res) => res.json())
      .then((data) => {
        if (process.env.NODE_ENV === 'development') setSubscriberLink(data.localSubscriber);
        if (process.env.NODE_ENV === 'test') setSubscriberLink(data.devSubscriber);
        if (process.env.NODE_ENV === 'production') setSubscriberLink(data.prodSubscriber);
      });
  }, []);
  return (
    <styled.PublishedSection
      label="VIEW ON MMIA"
      children={
        <Col>
          <Row className="view" onClick={() => window.open(`${subscriberLink}/view/${values.id}`)}>
            <FaExternalLinkAlt /> View on site
          </Row>
        </Col>
      }
    />
  );
};
