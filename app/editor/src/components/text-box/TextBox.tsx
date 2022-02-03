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
  return (
    <styled.TextBox
      className={className}
      height={height}
      width={width}
      backgroundColor={backgroundColor}
    >
      {children}
    </styled.TextBox>
  );
};
