import { PageSection } from 'components/section';
import React from 'react';
import { useSettings } from 'store/hooks';

import * as styled from './styled';

export const TopDomains: React.FC = () => {
  const settings = useSettings();

  return (
    <styled.TopDomains>
      <PageSection header="Top Domains">
        <iframe title="talkwalker-top-domains" src={settings.topDomainsUrl}></iframe>
      </PageSection>
    </styled.TopDomains>
  );
};
