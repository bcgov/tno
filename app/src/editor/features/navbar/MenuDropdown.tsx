import NavDropdown, { type NavDropdownProps } from 'react-bootstrap/NavDropdown';
import { useKeycloakWrapper, type Claim } from 'tno-core';

export interface IMenuDropdownProps extends NavDropdownProps {
  claim?: Claim[] | Claim;
}

export const MenuDropdown: React.FC<IMenuDropdownProps> = ({
  claim,
  children,
  title,
  ...props
}) => {
  const keycloak = useKeycloakWrapper();

  const hasClaim = keycloak.hasClaim(claim);

  return hasClaim ? <NavDropdown title={title}>{children}</NavDropdown> : <></>;
};
