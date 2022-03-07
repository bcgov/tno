import React from 'react';

export class LayoutErrorBoundary extends React.Component {
  constructor(props: React.Component) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: any) {
    // Update state so the next render will show the fallback UI.
    return { hasError: true };
  }

  componentDidCatch(error: any, errorInfo: any) {
    // TODO: Add logging of error.
    // You can also log the error to an error reporting service
    // logErrorToMyService(error, errorInfo);
  }

  render() {
    if ((this.state as any).hasError) {
      // You can render any custom fallback UI
      // TODO: handle error and display friendly message.
      return <h1>Something went wrong.</h1>;
    }

    return this.props.children;
  }
}
