import { UserMenu } from 'components';

import * as styled from './HeaderStyled';

interface IHeaderProps {
  authReady?: boolean;
}

/**
 * Provides a header element.
 * @param param0 Header element attributes.
 * @returns Header component.
 */
export const Header: React.FC<IHeaderProps> = ({ authReady = false }) => {
  return (
    <styled.Header>
      <a href="https://www2.gov.bc.ca/gov/content/home">
        <img alt="BC Gov logo" src="./assets/gov_bc_logo.svg" />
      </a>
      <div>
        <div className="title">Today's News Online</div>
        {authReady && <UserMenu />}
      </div>
    </styled.Header>
  );
};
