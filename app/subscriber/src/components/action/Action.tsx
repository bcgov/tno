import * as styled from './styled';

export interface IActionProps extends React.HTMLAttributes<HTMLDivElement> {
  /** A label to display */
  label?: string;
  /** An icon to display */
  icon?: React.ReactNode;
  /** The action is disabled */
  disabled?: boolean;
  /** Flex direction */
  direction?: 'row' | 'row-reverse' | 'column' | 'column-reverse';
}

/**
 * Provides an action/link button with a common style that includes an icon and label if provided.
 * @param param0 Component properties.
 * @returns Component
 */
export const Action: React.FC<IActionProps> = ({
  label,
  icon,
  className,
  children,
  onClick,
  ...rest
}) => {
  return (
    <styled.Action
      className={`action${className ? ` ${className}` : ''}`}
      onClick={(e) => {
        if (!rest.disabled) onClick?.(e);
      }}
      {...rest}
    >
      {icon}
      {label && <label>{label}</label>}
      {children}
    </styled.Action>
  );
};
