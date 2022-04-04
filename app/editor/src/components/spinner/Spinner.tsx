import React from 'react';

import * as styled from './SpinnerStyled';

/**
 * Provides a spinner component to indicate a loading state to the user.
 */
export const Spinner: React.FC<React.HTMLAttributes<HTMLOrSVGElement>> = () => {
  return (
    <styled.Spinner>
      <circle className="path" cx="25" cy="25" r="20" fill="none" strokeWidth="4" />
    </styled.Spinner>
  );
};
