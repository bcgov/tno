import { PageSection } from 'components/section';
import React from 'react';

import * as styled from './styled';

export const TopDomains: React.FC = () => {
  return (
    <styled.TopDomains>
      <PageSection header="Top Domains">
        <iframe
          title="talkwalker-top-domains"
          src="https://app.talkwalker.com/app/project/855b8383-dc00-429d-8207-6065d90ff277/cached/export_SocialMonitoring_bHIR978Y.html"
        ></iframe>
      </PageSection>
    </styled.TopDomains>
  );
};
