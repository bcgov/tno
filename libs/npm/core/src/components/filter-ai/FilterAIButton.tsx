import React from 'react';

import { Button, ButtonVariant } from '../button';

export interface IFilterAIButtonProps extends React.ButtonHTMLAttributes<HTMLButtonElement> {
  variant?: 'primary' | 'secondary';
}

/** A button the filter AI components render, so each app can supply its own styled button. */
export type FilterAIButtonComponent = React.JSXElementConstructor<IFilterAIButtonProps>;

/**
 * The default filter AI button, a tno-core Button.
 */
export const FilterAIButton: React.FC<IFilterAIButtonProps> = ({
  variant = 'primary',
  className,
  ...rest
}) => (
  <Button
    variant={variant === 'secondary' ? ButtonVariant.secondary : ButtonVariant.primary}
    className={className ? `filter-ai-button ${className}` : 'filter-ai-button'}
    {...rest}
  />
);
