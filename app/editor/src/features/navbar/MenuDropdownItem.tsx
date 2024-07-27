import NavDropdown from 'react-bootstrap/NavDropdown';
import { Link } from 'react-router-dom';
import { Claim, ILinkProps, useKeycloakWrapper } from 'tno-core';

export interface IMenuDropdownItemProps extends ILinkProps {
  claim?: Claim[] | Claim;
  children: React.ReactNode;
}

export const MenuDropdownItem: React.FC<IMenuDropdownItemProps> = ({
  claim,
  children,
  to,
  ...props
}) => {
  const keycloak = useKeycloakWrapper();

  const hasClaim = keycloak.hasClaim(claim);

  return hasClaim ? (
    <NavDropdown.Item as={Link} to={to}>
      {children}
    </NavDropdown.Item>
  ) : (
    <></>
  );
};
