import { Spinner } from '../spinners';
import * as styled from './styled';

export interface IBoxProps extends React.HTMLAttributes<HTMLDivElement> {
  /** Whether the area is loading */
  isLoading?: boolean;
}

/**
 * Provides a area component with a relative loading overlay.
 * @param param0 Component properties.
 * @returns Component.
 */
export const Box: React.FC<IBoxProps> = ({ isLoading, className, children, ...rest }) => {
  return (
    <styled.Box className={`box${className ? ` ${className}` : ''}`} {...rest}>
      {isLoading && (
        <div className="overlay">
          <Spinner />
        </div>
      )}
      {children}
    </styled.Box>
  );
};
