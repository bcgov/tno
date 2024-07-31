import React from 'react';

class TrackPageView extends React.Component {
  render() {
    window.snowplow('trackPageView');
    return null;
  }
}

export default TrackPageView;
