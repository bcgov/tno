import * as styled from './NotFoundStyled';
import SVG from 'react-inlinesvg';
import IconNotFound from './404.svg';

/**
 * NotFound provides a simple 404 Not Found error message.
 * @returns NotFound component.
 */
export const NotFound: React.FC = () => {
  return (
    <styled.NotFound>
      <div>
        <SVG src={IconNotFound} />
        <div>
          <h1>404: Page Not Found</h1>
          <p>This page does not exist.</p>
        </div>
      </div>
    </styled.NotFound>
  );
};
