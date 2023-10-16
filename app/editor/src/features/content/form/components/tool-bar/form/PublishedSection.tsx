import { IContentForm } from 'features/content/form/interfaces';
import React from 'react';
import { FaExternalLinkAlt } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useLookup } from 'store/hooks';
import { Col, Row } from 'tno-core';

import * as styled from './styled';

export interface IPublishedSectionProps {
  /** Form values */
  values: IContentForm;
}

export const PublishedSection: React.FC<IPublishedSectionProps> = ({ values }) => {
  const [{ settings }] = useLookup();
  const subscriberUrl = settings.find((i) => i.name === 'SubscriberUrl')?.value;

  return (
    <styled.PublishedSection
      label="VIEW ON MMI"
      children={
        <Col>
          <Row
            className="view"
            onClick={() => {
              if (!subscriberUrl) {
                toast.error('SubscriberUrl setting not found. Please set and try again.');
                return;
              }
              window.open(`${subscriberUrl}/view/${values.id}`);
            }}
          >
            <FaExternalLinkAlt size={25} />
          </Row>
        </Col>
      }
    />
  );
};
