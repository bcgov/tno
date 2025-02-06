import React from 'react';
import { toast, ToastOptions } from 'react-toastify';
import { Col } from 'tno-core';

import { useApp } from './useApp';

// Constants for error codes
const EXCEPTION_TEXT = {
  CONTENT_CONFLICT: 'ContentConflictException',
};

const HTTP_STATUS = {
  CONFLICT: 409,
};

// Common styles
const styles = {
  message: { margin: '0' },
  title: { margin: '0', fontWeight: 'bold' },
  container: { marginBottom: '8px' },
} as const;

export const useToastError = () => {
  const [state, api] = useApp();
  const [errors, setErrors] = React.useState(state.errors);

  React.useEffect(() => {
    if (state.errors.length) {
      setErrors((se) => {
        return state.errors;
      });
      if (state.errors) api.clearErrors();
    }
  }, [state.errors, api]);

  React.useEffect(() => {
    errors.forEach((e) => {
      const toastOptions: ToastOptions = {
        position: 'top-right',
      };

      // content conflict
      if (e.data?.type === EXCEPTION_TEXT.CONTENT_CONFLICT && e.status === HTTP_STATUS.CONFLICT) {
        toastOptions.autoClose = false;
        toastOptions.closeButton = true;
        toastOptions.className = 'concurrency-conflict-toast';

        // extract modified fields from error message
        const fieldsMatch = e.message.match(/^FIELDS:(.+)$/);
        const modifiedFields = fieldsMatch ? fieldsMatch[1].split(',').join(', ') : '';

        toast.error(
          <Col className="concurrency-conflict-message">
            <div>
              <div style={styles.container}>
                <p style={styles.title}>
                  Your changes could not be saved. Updates from another user could not be loaded.
                </p>
              </div>
              {modifiedFields && (
                <p style={styles.container}>
                  Modified fields: <span style={styles.title}>{modifiedFields}</span>
                </p>
              )}
              <p style={styles.message}>
                Please copy your changes elsewhere, then refresh the page to apply the updates.
              </p>
            </div>
          </Col>,
          toastOptions,
        );
      } else {
        toast.error(
          <Col>
            <p style={styles.message}>{e.message}</p>
            <p style={styles.message}>{e.detail}</p>
          </Col>,
          toastOptions,
        );
      }
    });
  }, [errors]);
};
