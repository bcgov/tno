import { IContentForm } from 'features/content/form/interfaces';
import React from 'react';
import { FaExternalLinkAlt } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useSettings } from 'store/hooks/admin';
import { Col, Row } from 'tno-core';

import * as styled from './styled';

export interface IPublishedSectionProps {
  /** Form values */
  values: IContentForm;
}

export const PublishedSection: React.FC<IPublishedSectionProps> = ({ values }) => {
  const [subscriberUrl, setSubscriberUrl] = React.useState<string>();
  const [{ settings }, api] = useSettings();

  React.useEffect(() => {
    api.findAllSettings().then((data) => {
      setSubscriberUrl(data.find((i) => i.name === 'SubscriberUrl')?.value);
    });
  }, [api, settings, subscriberUrl]);

  return (
    <styled.PublishedSection
      label="VIEW ON MMIA"
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
            <FaExternalLinkAlt /> View on site
          </Row>
        </Col>
      }
    />
  );
};
