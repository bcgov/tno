import { collector_production, collector_sandbox } from './snowplow-constants';
// <!-- Snowplow starts plowing - Standalone vE.2.14.0 -->
(function (p, l, o, w, i, n, g) {
  if (!p[i]) {
    p.GlobalSnowplowNamespace = p.GlobalSnowplowNamespace || [];
    p.GlobalSnowplowNamespace.push(i);
    p[i] = function () {
      (p[i].q = p[i].q || []).push(arguments);
    };
    p[i].q = p[i].q || [];
    n = l.createElement(o);
    g = l.getElementsByTagName(o)[0];
    n.async = 1;
    n.src = w;
    g.parentNode.insertBefore(n, g);
  }
})(
  window,
  document,
  'script',
  'https://www2.gov.bc.ca/StaticWebResources/static/sp/sp-2-14-0.js',
  'snowplow',
);
var collector;
if (process.env.NODE_ENV === 'development' || process.env.NODE_ENV === 'test') {
  collector = collector_sandbox;
} else {
  collector = collector_production;
}
window.snowplow('newTracker', 'rt', collector, {
  appId: 'Snowplow_standalone_MMI',
  cookieLifetime: 86400 * 548,
  platform: 'web',
  post: true,
  forceSecureTracker: true,
  contexts: {
    webPage: true,
    performanceTiming: true,
  },
});
window.snowplow('enableActivityTracking', 30, 30); // Ping every 30 seconds after 30 seconds
window.snowplow('enableLinkClickTracking');
// <!-- Snowplow stops plowing -->
