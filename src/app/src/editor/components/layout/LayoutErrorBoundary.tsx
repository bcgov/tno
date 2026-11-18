import React from 'react';

export class LayoutErrorBoundary extends React.Component<any, any> {
  constructor(props: React.Component) {
    super(props);
    this.state = { hasError: false };
  }

  static getDerivedStateFromError(error: any) {
    // Update state so the next render will show the fallback UI.
    return { hasError: true, error };
  }

  componentDidCatch(error: any, errorInfo: any) {
    // TODO: Add logging of error.
    // You can also log the error to an error reporting service
    // logErrorToMyService(error, errorInfo);
    this.setState(LayoutErrorBoundary.getDerivedStateFromError(error));
  }

  render() {
    if ((this.state as any).hasError) {
      // You can render any custom fallback UI
      // TODO: handle error and display friendly message.
      return (
        <div className="error-boundary">
          <div>
            <h1>Error</h1>
            <div>
              <p>We're sorry you discovered an unexpected bug. Please refresh your page.</p>
              <p>
                If this bug continues please contact{' '}
                <a href="mailto:support@tno.gov.bc">support@tno.gov.bc</a>.
              </p>
              <p className="error">{this.state.error.message}</p>
            </div>
          </div>
        </div>
      );
    }

    return this.props.children;
  }
}
