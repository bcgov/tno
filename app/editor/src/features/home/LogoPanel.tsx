import * as styled from './styled';

export interface ILogoPanelProps {
  /**
   * choose the width of the panel
   */
  width?: string;
  /**
   * choose the background colour of the panel
   */
  backgroundColor?: string;
}

/**
 * TextBox provides a customizable container to place informative information in.
 * @param width Div width element attribute.
 * @param backgroundColor Div background-color element attribute.
 * @returns LogoPanel component.
 */
export const LogoPanel: React.FC<ILogoPanelProps> = ({ width, backgroundColor }) => {
  return (
    <styled.LogoPanel width={width} backgroundColor={backgroundColor}>
      <img
        alt="BC Gov logo"
        className="logo"
        height={39.67}
        width={150}
        src={'/assets/gov_bc_logo.svg'}
      />
    </styled.LogoPanel>
  );
};
