// The Snowplow tracker that components/snowplow/snowplow.js installs on window.
declare global {
  interface Window {
    snowplow: any;
  }
}

export {};
