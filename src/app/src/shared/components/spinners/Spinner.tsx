import { SpinnerVariant } from '.';
import * as styled from './styled';

export interface ISpinnerProps extends React.HtmlHTMLAttributes<HTMLDivElement> {
  /**
   * Spinner variant style options.
   */
  variant?: SpinnerVariant;
  /**
   * The size of the spinner (default: 2rem).
   */
  size?: string;
}

/**
 * Spinner provides a animated loading icon.
 * @param param0 Div element attributes.
 * @returns Spinner component.
 */
export const Spinner: React.FC<ISpinnerProps> = ({
  variant = SpinnerVariant.primary,
  size = '2rem',
  className,
  ...rest
}) => {
  return (
    <styled.Spinner
      className={`spinner${!!className ? ` ${className}` : ''}`}
      variant={variant}
      size={size}
      {...rest}
    >
      <styled.Loading>Loading...</styled.Loading>
    </styled.Spinner>
  );
};
