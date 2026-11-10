import { useKeycloak } from '@react-keycloak/web';
import React from 'react';
import { useNavigate, useParams } from 'react-router-dom';

import ReportInstancePreview from './ReportInstancePreview';

/**
 * Temporary redirect component for report/instances view
 * Redirects unauthenticated users to external URL
 * Authenticated users are redirected to the normal view
 */
export const ReportInstancesRedirect: React.FC = () => {
  const { keycloak } = useKeycloak();
  const { id } = useParams<{ id: string }>();
  const navigate = useNavigate();

  React.useEffect(() => {
    if (!keycloak?.authenticated) {
      // Redirect unauthenticated users to external URL
      // TODO: Replace EXTERNAL_URL_HERE with your actual external URL
      const externalUrl = `https://mmi.gov.bc.ca/report/instances/${id}/view`;
      window.location.href = externalUrl;
    } else {
      // Redirect authenticated users to the normal view
      navigate(`/report/instances/${id}/view`, { replace: true });
    }
  }, [keycloak?.authenticated, id, navigate]);

  return <ReportInstancePreview />; // This component just handles the redirect
};

export default ReportInstancesRedirect;
