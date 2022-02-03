import * as styled from './styled';

export interface IInfoPanelProps {
  /**
   * choose the width of the panel
   */
  width?: string;
  /**
   * choose the background colour of the panel
   */
  backgroundColor?: string;
  /**
   * determine the elements to be displayed in the info panel
   */
  children?: React.ReactNode;
  /**
   * determine whether to add a button to this panel
   */
  roundedEdges?: boolean;
}

/**
 * InfoPanel provides a customizable container to place informative information in.
 * @param width Div width element attribute.
 * @param backgroundColor Div background-color element attribute.
 * @returns InfoPanel component.
 */
export const InfoPanel: React.FC<IInfoPanelProps> = ({ width, backgroundColor, children }) => {
  return (
    <styled.InfoPanel width={width} backgroundColor={backgroundColor}>
      {children}
    </styled.InfoPanel>
  );
};
