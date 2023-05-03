import { defaultAlert } from 'features/admin/alerts/constants';
import React from 'react';
import { useAlerts } from 'store/hooks/admin';
import { IAlertModel } from 'tno-core';

export const InfoText: React.FC = () => {
  const [, api] = useAlerts();
  const [alert, setAlert] = React.useState<IAlertModel>(defaultAlert);

  React.useEffect(() => {
    api.findAllAlerts().then((data) => {
      if (data.length > 0) setAlert(data[0]);
    });
    // only want to run on mount
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, []);

  return (
    <div className="info">
      <p>
        Media Monitoring Insights & Analysis is a service that allows people to see all of BC’s news
        at a glance. Some of its key features include:
      </p>
      <ul>
        <li>Aggregation of all newspapers, radio shows, and online articles.</li>
        <li>BC’s top stories as they break.</li>
        <li>Articles related to major stories.</li>
      </ul>
      <p>{alert.message ?? 'There are currently no pinned messages.'}</p>
      <div className="email">
        <a style={{ marginTop: 25 }} href="mailto:tnonews-help@gov.bc.ca">
          tnonews-help@gov.bc.ca
        </a>
      </div>
    </div>
  );
};
