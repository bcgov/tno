// import 'bootstrap/dist/css/bootstrap.min.css';

import Container from 'react-bootstrap/Container';
import Nav from 'react-bootstrap/Nav';
import Navbar from 'react-bootstrap/Navbar';
import NavDropdown from 'react-bootstrap/NavDropdown';
import { FaClipboard, FaCogs, FaSearch } from 'react-icons/fa';
import {
  FaClipboardList,
  FaCloudArrowDown,
  FaEnvelope,
  FaExclamation,
  FaFeather,
  FaFolder,
  FaHouse,
  FaMessage,
  FaMoneyBillWave,
  FaMoon,
  FaNewspaper,
  FaSun,
  FaUsers,
} from 'react-icons/fa6';
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
                  <FaNewspaper /> Papers
                </NavDropdown.Item>
                <NavDropdown.Divider />
                <NavDropdown.Item as={Link} to="/transcriptions">
                  <FaFeather /> Transcript Queue
                </NavDropdown.Item>
              </NavDropdown>
              <NavDropdown title="Report Building" id="basic-nav-dropdown">
                <Container>
                  <MenuDropdownItem claim={Claim.administrator} to="/admin/reports/dashboard">
                    Dashboard
                  </MenuDropdownItem>
                  <MenuDropdownItem claim={Claim.administrator} to="/admin/products">
                    <FaEnvelope /> MMI Products
                  </MenuDropdownItem>
                  <MenuDropdownItem claim={Claim.administrator} to="/admin/reports">
                    <FaClipboard /> Reports
                  </MenuDropdownItem>
                  <MenuDropdownItem claim={Claim.administrator} to="/admin/filters">
                    <FaSearch /> Filters
                  </MenuDropdownItem>
                  <MenuDropdownItem claim={Claim.administrator} to="/admin/folders">
                    <FaFolder /> Folders
                  </MenuDropdownItem>
                  {isAdmin && <NavDropdown.Divider />}
                  <MenuDropdownItem to="/reports/event-of-the-day">
                    <FaSun /> Event of the Day
                  </MenuDropdownItem>
                  <MenuDropdownItem claim={Claim.administrator} to="/admin/topics">
                    Edit Topics
                  </MenuDropdownItem>
                  <MenuDropdownItem claim={Claim.administrator} to="/admin/topic-scores">
                    Configure Topic Scoring
                  </MenuDropdownItem>
                  <NavDropdown.Divider />
                  <MenuDropdownItem to="/reports/av/evening-overview">
                    <FaMoon /> Evening Overview
                  </MenuDropdownItem>
                  <MenuDropdownItem claim={Claim.administrator} to="/admin/av/evening-overview">
                    Evening Overview Templates
                  </MenuDropdownItem>
                  {isAdmin && <NavDropdown.Divider />}
                  <MenuDropdownItem claim={Claim.administrator} to="/reports/cbra">
                    <FaMoneyBillWave /> CBRA Report
                  </MenuDropdownItem>
                </Container>
              </NavDropdown>
              {isAdmin && (
                <NavDropdown title="Notifications Building" id="basic-nav-dropdown">
                  <MenuDropdownItem claim={Claim.administrator} to="/admin/notifications/dashboard">
                    Dashboard
                  </MenuDropdownItem>
                  <NavDropdown.Item as={Link} to="/admin/notifications">
                    <FaMessage /> Notifications
                  </NavDropdown.Item>
                </NavDropdown>
              )}
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
                  <FaCloudArrowDown /> Ingest Services
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
                <NavDropdown.Item as={Link} to="/admin/connections">
                  Data Connections
                </NavDropdown.Item>
              </MenuDropdown>
              <MenuDropdown title="System Settings" claim={Claim.administrator}>
                <NavDropdown.Item as={Link} to="/admin/users">
                  <FaUsers /> Manage Users
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/admin/system-messages">
                  <FaExclamation />
                  System Messages
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/admin/work/orders">
                  <FaClipboardList />
                  Work Orders
                </NavDropdown.Item>
                <NavDropdown.Item as={Link} to="/admin/settings">
                  <FaCogs /> System Configuration
                </NavDropdown.Item>
              </MenuDropdown>
            </Nav>
          </Navbar.Collapse>
        </Container>
      </Navbar>
    </styled.Menu>
  );
};
