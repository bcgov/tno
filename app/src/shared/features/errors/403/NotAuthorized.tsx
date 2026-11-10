import IconNotAuthorized from '../../../assets/403.svg';
import * as styled from './NotAuthorizedStyled';

/**
 * NotAuthorized provides a simple 403 Not Authorized error message.
 * @returns NotAuthorized component.
 */
export const NotAuthorized: React.FC = () => {
  return (
    <styled.NotAuthorized>
      <div>
        <IconNotAuthorized />
        <div>
          <h1>403: Not Authorized</h1>
          <p>You do not have access to this.</p>
        </div>
      </div>
    </styled.NotAuthorized>
  );
};
