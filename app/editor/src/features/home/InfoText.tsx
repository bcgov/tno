export const InfoText: React.FC = () => {
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
      <p>
        The following is a pinned message and announcement for all subscribers to the Media
        Monitoring Insights & Analysis Service. Please be aware that system maintenance will occur
        this Friday, from 8:00AM to 5PM.
      </p>
      <div className="email">
        <a style={{ marginTop: 25 }} href="mailto:tnonews-help@gov.bc.ca">
          tnonews-help@gov.bc.ca
        </a>
      </div>
    </div>
  );
};
