import { InputHTMLAttributes } from 'react';

import { TextVariant } from '..';
import * as styled from './TextAreaStyled';

export interface ITextAreaProps extends InputHTMLAttributes<HTMLTextAreaElement> {
  /**
   * The styled variant.
   */
  variant?: TextVariant;
  /**
   * The tooltip to show on hover.
   */
  tooltip?: string;
}

export const TextArea: React.FC<ITextAreaProps> = ({
  variant = TextVariant.primary,
  tooltip,
  children,
  className,
  ...rest
}) => {
  return (
    <styled.TextArea
      variant={variant}
      {...rest}
      className={`${className}`}
      data-for="main"
      data-tip={tooltip}
    >
      {children}
    </styled.TextArea>
  );
};
