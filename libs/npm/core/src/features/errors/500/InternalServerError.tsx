import * as styled from './InternalServerErrorStyled';
import SVG from 'react-inlinesvg';
import IconInternalServerError from './500.svg';

/**
 * InternalServerError provides a simple 500 Internal Server Error message.
 * @returns InternalServerError component.
 */
export const InternalServerError: React.FC = () => {
  return (
    <styled.InternalServerError>
      <div>
        <SVG src={IconInternalServerError} />
        <div>
          <h1>500: Internal Server Error</h1>
          <p>An unexpected error has occurred.</p>
        </div>
      </div>
    </styled.InternalServerError>
  );
};
