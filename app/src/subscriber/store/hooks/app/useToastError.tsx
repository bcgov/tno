import React from 'react';
import { toast } from 'react-toastify';
import { Col } from 'tno-core';

import { useApp } from './useApp';

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
      toast.error(
        <Col>
          {e.status && <h3>{`${e.status}${e.statusText ? ` - ${e.statusText}` : ''}`}</h3>}
          <p>{e.message}</p>
          {e.detail && <p>{e.detail}</p>}
        </Col>,
        {},
      );
    });
  }, [errors]);
};
