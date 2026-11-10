import IconNotFound from '../../../assets/404.svg';
import * as styled from './NotFoundStyled';

/**
 * NotFound provides a simple 404 Not Found error message.
 * @returns NotFound component.
 */
export const NotFound: React.FC = () => {
  return (
    <styled.NotFound>
      <div>
        <IconNotFound />
        <div>
          <h1>404: Page Not Found</h1>
          <p>This page does not exist.</p>
        </div>
      </div>
    </styled.NotFound>
  );
};
