import { ISpinnerProps, Spinner } from '.';
import * as styled from './styled';

export interface ILoaderProps
  extends ISpinnerProps,
    Omit<React.HTMLAttributes<HTMLDivElement>, 'size'> {
  /** Whether the overlay loader is visible */
  visible?: boolean;
}

/**
 * Component for displaying loading overlay with a spinner.
 * @param param0 Component properties.
 * @returns Component.
 */
export const Loader: React.FC<ILoaderProps> = ({ visible, variant, size, className, ...rest }) => {
  return visible ? (
    <styled.Loader className={`overlay${!!className ? ` ${className}` : ''}`} {...rest}>
      <Spinner variant={variant} size={size} />
    </styled.Loader>
  ) : null;
};
