// import 'bootstrap/dist/css/bootstrap.min.css';

import Container from 'react-bootstrap/Container';
import Nav from 'react-bootstrap/Nav';
import Navbar from 'react-bootstrap/Navbar';
import NavDropdown from 'react-bootstrap/NavDropdown';
import { FaHouse } from 'react-icons/fa6';
import { Link } from 'react-router-dom';
import { Claim, Row, useKeycloakWrapper } from 'tno-core';

import { MenuDropdown } from './MenuDropdown';
import { MenuDropdownItem } from './MenuDropdownItem';
import * as styled from './styled';

export const Menu: React.FC = () => {
  const keycloak = useKeycloakWrapper();

  const isAdmin = keycloak.hasClaim(Claim.administrator);

  return (
    <styled.Menu>
      <Navbar expand="lg">
        <Container fluid>
          <Navbar.Brand as={Link} to="/contents">
            <Row alignItems="center" gap="0.25rem">
              <FaHouse />
              Home
            </Row>
          </Navbar.Brand>
          <Navbar.Toggle aria-controls="basic-navbar-nav" />
          <Navbar.Collapse id="basic-navbar-nav">
            <Nav className="me-auto">
              <NavDropdown title="Content" id="basic-nav-dropdown">
                <NavDropdown.Item as={Link} to="/contents">
                  All Content
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/papers">
                  Papers
                </NavDropdown.Item>
                <NavDropdown.Divider />
                <NavDropdown.Item as={Link} to="/transcriptions">
                  Transcript Queue
                </NavDropdown.Item>
              </NavDropdown>
              <NavDropdown title="Report Building" id="basic-nav-dropdown">
                <Container>
                  <MenuDropdownItem claim={Claim.administrator} to="/admin/filters">
                    Filters
                  </MenuDropdownItem>
                  <MenuDropdownItem claim={Claim.administrator} to="/admin/folders">
                    Folders
                  </MenuDropdownItem>
                  <NavDropdown.Item as={Link} to="/reports/av/evening-overview">
                    Evening Overview
                  </NavDropdown.Item>
                  {!isAdmin && (
                    <NavDropdown.Item as={Link} to="/reports/event-of-the-day">
                      Event of the Day
                    </NavDropdown.Item>
                  )}
                  <MenuDropdown title="Event of the Day" claim={Claim.administrator}>
                    <NavDropdown.Item as={Link} to="/reports/event-of-the-day">
                      Event of the Day
                    </NavDropdown.Item>
                    <MenuDropdownItem claim={Claim.administrator} to="/admin/topics">
                      Edit Topics
                    </MenuDropdownItem>
                    <MenuDropdownItem claim={Claim.administrator} to="/admin/topic-scores">
                      Configure Topic Scoring
                    </MenuDropdownItem>
                  </MenuDropdown>
                  <MenuDropdownItem claim={Claim.administrator} to="/admin/products">
                    MMI Products
                  </MenuDropdownItem>
                  <MenuDropdownItem claim={Claim.administrator} to="/reports/cbra">
                    CBRA Report
                  </MenuDropdownItem>
                  <MenuDropdown title="Report Configuration" claim={Claim.administrator}>
                    <NavDropdown.Item as={Link} to="/admin/reports">
                      Manage Reports
                    </NavDropdown.Item>
                    <NavDropdown.Item as={Link} to="/admin/av/evening-overview">
                      Evening Overview Templates
                    </NavDropdown.Item>
                  </MenuDropdown>
                </Container>
              </NavDropdown>
              <MenuDropdown title="Content Configuration" claim={Claim.administrator}>
                <NavDropdown.Item as={Link} to="/admin/sources">
                  Media Sources
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/admin/programs">
                  Shows & Programs
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/admin/tags">
                  Tags
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/admin/media-types">
                  Media Types
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/admin/contributors">
                  Columnist & Pundits
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/admin/ministers">
                  Ministers
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/admin/actions">
                  Actions
                </NavDropdown.Item>
              </MenuDropdown>
              <MenuDropdown title="Data Import" claim={Claim.administrator}>
                <NavDropdown.Item as={Link} to="/admin/dashboard">
                  Services Dashboard
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/admin/ingests">
                  Ingest Services
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/admin/ingest/types">
                  Ingest Types
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/admin/licences">
                  Media Licenses
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/admin/data/locations">
                  Data Locations
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/admin/connection">
                  Data Connections
                </NavDropdown.Item>
              </MenuDropdown>
              <MenuDropdown title="System Settings" claim={Claim.administrator}>
                <NavDropdown.Item as={Link} to="/admin/users">
                  Manage Users
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/admin/system-message">
                  System Message
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/admin/notifications">
                  Notifications
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/admin/work/orders">
                  Work Orders
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/admin/settings">
                  System Configuration
                </NavDropdown.Item>
              </MenuDropdown>
            </Nav>
          </Navbar.Collapse>
        </Container>
      </Navbar>
    </styled.Menu>
  );
};
