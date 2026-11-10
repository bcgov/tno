import IconInternalServerError from '../../../assets/500.svg';
import * as styled from './InternalServerErrorStyled';

/**
 * InternalServerError provides a simple 500 Internal Server Error message.
 * @returns InternalServerError component.
 */
export const InternalServerError: React.FC = () => {
  return (
    <styled.InternalServerError>
      <div>
        <IconInternalServerError />
        <div>
          <h1>500: Internal Server Error</h1>
          <p>An unexpected error has occurred.</p>
        </div>
      </div>
    </styled.InternalServerError>
  );
};
