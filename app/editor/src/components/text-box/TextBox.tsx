import { useEffect, useState } from 'react';

import * as styled from './styled';

export interface ITextBoxProps {
  /**
   * choose the height of the text box
   */
  height?: string;
  /**
   * choose the width of the text box
   */
  width?: string;
  /**
   * choose the background colour of the checkbox
   */
  backgroundColor?: string;
  /**
   * pass children to the text box
   */
  children?: React.ReactNode;
  /**
   * className for the component
   */
  className?: string;

  useMobileMode?: boolean;
}

/**
 * TextBox provides a customizable container to place informative information in.
 * @param height Div height element attribute.
 * @param width Div width element attribute.
 * @param backgroundColor Div background-color element attribute.
 * @returns TextBox component.
 */
export const TextBox: React.FC<ITextBoxProps> = ({
  height,
  width,
  backgroundColor,
  children,
  className,
}) => {
  const [mobileMode, setMobileMode] = useState(false);

  useEffect(() => {
    const handleResize = () => {
      const isMobile = window.navigator.maxTouchPoints === 1;
      setMobileMode(isMobile);
    };
    window.addEventListener('resize', handleResize);
    return () => {
      window.removeEventListener('resize', handleResize);
    };
  }, []);

  return (
    <styled.TextBox
      className={className}
      height={height}
      width={width}
      backgroundColor={backgroundColor}
      useMobileMode={mobileMode}
    >
      {children}
    </styled.TextBox>
  );
};
