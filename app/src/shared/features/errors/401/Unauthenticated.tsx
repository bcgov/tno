import IconUnauthenticated from '../../../assets/401.svg';
import * as styled from './UnauthenticatedStyled';

/**
 * Unauthenticated provides a simple 401 Unauthenticated error message.
 * @returns Unauthenticated component.
 */
export const Unauthenticated: React.FC = () => {
  return (
    <styled.Unauthenticated>
      <div>
        <IconUnauthenticated />
        <div>
          <h1>401: Unauthenticated</h1>
          <p>You need to login.</p>
        </div>
      </div>
    </styled.Unauthenticated>
  );
};
