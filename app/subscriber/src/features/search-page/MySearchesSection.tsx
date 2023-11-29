import { PageSection } from 'components/section';
import { MySearches } from 'features/my-searches';

export const MySearchesSection: React.FC = () => {
  return (
    <PageSection header="My Searches">
      <MySearches />
    </PageSection>
  );
};
