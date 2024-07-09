import { PageSection } from 'components/section';
import React from 'react';
import { Col } from 'tno-core';

import * as styled from './styled/Help';

export const Help: React.FC = () => {
  return (
    <styled.Help>
      <PageSection header="Help">
        <Col className="help-content">
          <div>Help Page</div>
        </Col>
      </PageSection>
    </styled.Help>
  );
};
