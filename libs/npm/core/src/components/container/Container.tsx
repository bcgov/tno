import { Spinner } from '../spinners';
import * as styled from './styled';

export interface IContainerProps extends React.HTMLAttributes<HTMLDivElement> {
  /** Whether the container is loading */
  isLoading?: boolean;
}

/**
 * Provides a container component with a relative loading overlay.
 * @param param0 Component properties.
 * @returns Component.
 */
export const Container: React.FC<IContainerProps> = ({
  isLoading,
  className,
  children,
  ...rest
}) => {
  return (
    <styled.Container className={`container${className ? ` ${className}` : ''}`} {...rest}>
      {isLoading && (
        <div className="overlay">
          <Spinner />
        </div>
      )}
      {children}
    </styled.Container>
  );
};
