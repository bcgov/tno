import {
  FaArrowRotateLeft,
  FaArrowRotateRight,
  FaArrowsSpin,
  FaArrowUpRightFromSquare,
  FaChevronDown,
  FaChevronUp,
  FaFloppyDisk,
  FaMinus,
  FaPen,
  FaPlus,
  FaTrash,
  FaWandSparkles,
  FaX,
} from 'react-icons/fa6';

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
  /** Default variants have predefined icons. */
  variant?:
    | 'icon'
    | 'close'
    | 'save'
    | 'trash'
    | 'pen'
    | 'open'
    | 'refresh'
    | 'chevron-down'
    | 'chevron-up'
    | 'undo'
    | 'redo'
    | 'plus'
    | 'minus'
    | 'create-new';
  /** Size of icon */
  size?: string;
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
  variant = 'icon',
  size = '1em',
  onClick,
  ...rest
}) => {
  let variantIcon = icon;
  if (variant === 'close') variantIcon = <FaX />;
  else if (variant === 'save') variantIcon = <FaFloppyDisk />;
  else if (variant === 'trash') variantIcon = <FaTrash />;
  else if (variant === 'pen') variantIcon = <FaPen />;
  else if (variant === 'open') variantIcon = <FaArrowUpRightFromSquare />;
  else if (variant === 'refresh') variantIcon = <FaArrowsSpin />;
  else if (variant === 'chevron-up') variantIcon = <FaChevronUp />;
  else if (variant === 'chevron-down') variantIcon = <FaChevronDown />;
  else if (variant === 'undo') variantIcon = <FaArrowRotateLeft />;
  else if (variant === 'redo') variantIcon = <FaArrowRotateRight />;
  else if (variant === 'plus') variantIcon = <FaPlus />;
  else if (variant === 'minus') variantIcon = <FaMinus />;
  else if (variant === 'create-new') variantIcon = <FaWandSparkles />;

  return (
    <styled.Action
      className={`action${className ? ` ${className}` : ''}`}
      onClick={(e) => {
        if (!rest.disabled) onClick?.(e);
      }}
      variant={variant}
      size={size}
      {...rest}
    >
      {variantIcon}
      {label && <label>{label}</label>}
      {children}
    </styled.Action>
  );
};
