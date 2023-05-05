import { defaultAlert } from 'features/admin/alerts/constants';
import React from 'react';
import { useAnonAlerts } from 'store/hooks';
import { IAlertModel, Show } from 'tno-core';

export const InfoText: React.FC = () => {
  const [, api] = useAnonAlerts();
  const [alert, setAlert] = React.useState<IAlertModel>(defaultAlert);

  React.useEffect(() => {
    api.findAlert().then((data) => {
      if (!!data) setAlert(data);
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
      <Show visible={alert.isEnabled}>
        <p className="alert-message">{alert.message ?? ''}</p>
      </Show>
      <div className="email">
        <a style={{ marginTop: 25 }} href="mailto:tnonews-help@gov.bc.ca">
          tnonews-help@gov.bc.ca
        </a>
      </div>
    </div>
  );
};
