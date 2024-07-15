import { FaLink } from 'react-icons/fa';
import { NavigateOptions, useNavigate } from 'react-router-dom';

import * as styled from './styled';

export interface ILinkProps extends React.HTMLAttributes<HTMLDivElement> {
  /** Route to navigate to. */
  to: string;
  /** Navigation options. */
  options?: NavigateOptions;
  /** use icon*/
  icon?: JSX.Element;
}

/**
 * Provides a link that can open a new tab when ctrl+click is pressed.
 * Default behaviour is to open with react-router navigate.
 * @param param0 Component props.
 * @returns Component.
 */
export const Link: React.FC<ILinkProps> = ({
  to,
  options,
  className,
  children,
  icon = <FaLink />,
  onClick,
  ...rest
}) => {
  const navigate = useNavigate();

  return (
    <styled.Link
      className={`link${className ? ` ${className}` : ''}`}
      onClick={(e) => {
        if (e.ctrlKey) window.open(to, '_blank');
        else navigate(to, options);
      }}
      {...rest}
    >
      {icon}
      {children}
    </styled.Link>
  );
};
