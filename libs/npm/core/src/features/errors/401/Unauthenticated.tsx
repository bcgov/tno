import * as styled from './UnauthenticatedStyled';
import SVG from 'react-inlinesvg';
import IconUnauthenticated from './401.svg';

/**
 * Unauthenticated provides a simple 401 Unauthenticated error message.
 * @returns Unauthenticated component.
 */
export const Unauthenticated: React.FC = () => {
  return (
    <styled.Unauthenticated>
      <div>
        <SVG src={IconUnauthenticated} />
        <div>
          <h1>401: Unauthenticated</h1>
          <p>You need to login.</p>
        </div>
      </div>
    </styled.Unauthenticated>
  );
};
