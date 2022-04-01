import React from 'react';
import SVG from 'react-inlinesvg';
import ReactTooltip from 'react-tooltip';

import IconToggle from '../../assets/navbar-toggle-icon.svg';
import { useKeycloakWrapper } from '../../hooks';
import { Button } from '..';
import { MenuContext, MenuStatus } from '.';
import * as styled from './styled';

export interface IMenuToggleProps {
  /**
   * Width of the button.
   */
  width?: string;
  /**
   * Hieght of the button.
   */
  height?: string;
}

/**
 * MenuToggle provides a toggle button for expanding and shrinking the menu bar.
 * @returns MenuToggle compoent.
 */
export const MenuToggle: React.FC<IMenuToggleProps> = ({ width = '45px', height = '32px' }) => {
  const keycloak = useKeycloakWrapper();
  const { status, setStatus } = React.useContext(MenuContext);
  let tip = '';
  switch (status) {
    case MenuStatus.full:
      tip = 'Minimize Menu';
      break;
    case MenuStatus.narrow:
      tip = 'Hide Menu';
      break;
    case MenuStatus.hidden:
    default:
      tip = 'Show Menu';
      break;
  }

  React.useEffect(() => {
    ReactTooltip.hide();
    ReactTooltip.rebuild();
  }, [status]);

  return keycloak.authenticated ? (
    <styled.MenuToggle
      width={width}
      height={height}
      data-for="main-tooltip-right"
      data-tip={tip}
      className="toggle-icon"
      onClick={() => {
        switch (status) {
          case MenuStatus.full:
            return setStatus(MenuStatus.narrow);
          case MenuStatus.narrow:
            return setStatus(MenuStatus.hidden);
          case MenuStatus.hidden:
          default:
            return setStatus(MenuStatus.full);
        }
      }}
    >
      <Button>
        <SVG src={IconToggle} />
      </Button>
    </styled.MenuToggle>
  ) : (
    <div style={{ width, height }}>&nbsp;</div>
  );
};
