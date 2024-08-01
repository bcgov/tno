import React from 'react';

declare global {
    interface Window {
        snowplow:any;
    }
}

class TrackPageView extends React.Component {
  render() {
    window.snowplow('trackPageView');
    return null;
  }
}

export default TrackPageView;
