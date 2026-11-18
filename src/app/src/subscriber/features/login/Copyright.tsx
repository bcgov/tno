import React from 'react';

import * as styled from './styled';

export const Copyright: React.FC = () => {
  return (
    <styled.Copyright className="copyright">
      <b>Copyright info:</b>
      <p>
        This account grants you access to copyrighted material for your own use. It does not grant
        you permission to fix, copy, reproduce or archive any of the material contained within.{' '}
        <br /> <br />
        You cannot redistribute this information to anyone without violating your copyright
        agreement.
      </p>
    </styled.Copyright>
  );
};
