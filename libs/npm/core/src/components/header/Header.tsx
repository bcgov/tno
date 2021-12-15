import { MenuToggle, UserMenu } from '..';
import * as styled from './HeaderStyled';

interface IHeaderProps extends React.HTMLAttributes<HTMLDivElement> {
  /**
   * Is authentication client ready?
   */
  authReady?: boolean;
  /**
   * The site name.
   */
  name: string;
}

/**
 * Provides a header element.
 * @param param0 Header element attributes.
 * @returns Header component.
 */
export const Header: React.FC<IHeaderProps> = ({ name, authReady = true, children, ...rest }) => {
  return (
    <styled.Header {...rest}>
      {authReady && <MenuToggle />}
      <div>
        <a href="https://www2.gov.bc.ca/gov/content/home">
          <img alt="BC Gov logo" src={process.env.PUBLIC_URL + '/assets/gov_bc_logo.svg'} />
        </a>
      </div>
      <div>
        <div className="title">{name}</div>
        {authReady && <UserMenu />}
      </div>
    </styled.Header>
  );
};
