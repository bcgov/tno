import { PageSection } from 'components/section';
import React from 'react';
import { FaVideo } from 'react-icons/fa';
import { Col, Row } from 'tno-core';

import * as styled from './styled/Help';

export const Help: React.FC = () => {
  return (
    <styled.Help>
      <PageSection header="Help" includeHeaderIcon>
        <Row>
          <Col className="help-content">
            <h2>
              <FaVideo className="list-icon" />
              <span>Successfully logging into MMI</span>
            </h2>
            <Row>
              <Col className="media-playback">
                <video controls src={`${process.env.PUBLIC_URL}/public/videos/login.mp4`} />
              </Col>
            </Row>
          </Col>
        </Row>
        <Row>
          <Col className="help-content">
            <h2>
              <FaVideo className="list-icon" />
              <span>Failed login attempt</span>
            </h2>
            <Row>
              <Col className="media-playback">
                <video controls src={`${process.env.PUBLIC_URL}/public/videos/failedlogin.mp4`} />
              </Col>
            </Row>
          </Col>
        </Row>
        <Row>
          <Col className="help-content">
            <h2>
              <FaVideo className="list-icon" />
              <span>Home Page Tour</span>
            </h2>
            <Row>
              <Col className="media-playback">
                <video controls src={`${process.env.PUBLIC_URL}/public/videos/homepagetour.mp4`} />
              </Col>
            </Row>
          </Col>
        </Row>
        <Row>
          <Col className="help-content">
            <h2>
              <FaVideo className="list-icon" />
              <span>My Minister</span>
            </h2>
            <Row>
              <Col className="media-playback">
                <video controls src={`${process.env.PUBLIC_URL}/public/videos/myminister.mp4`} />
              </Col>
            </Row>
          </Col>
        </Row>
        <Row>
          <Col className="help-content">
            <h2>
              <FaVideo className="list-icon" />
              <span>Simple and Complex Searches</span>
            </h2>
            <Row>
              <Col className="media-playback">
                <video
                  controls
                  src={`${process.env.PUBLIC_URL}/public/videos/simplecomplexsearch.mp4`}
                />
              </Col>
            </Row>
          </Col>
        </Row>
        <Row>
          <Col className="help-content">
            <h2>
              <FaVideo className="list-icon" />
              <span>Creating a Saved Search</span>
            </h2>
            <Row>
              <Col className="media-playback">
                <video
                  controls
                  src={`${process.env.PUBLIC_URL}/public/videos/creatingsavedsearch.mp4`}
                />
              </Col>
            </Row>
          </Col>
        </Row>
        <Row>
          <Col className="help-content">
            <h2>
              <FaVideo className="list-icon" />
              <span>Editing a Saved Search</span>
            </h2>
            <Row>
              <Col className="media-playback">
                <video
                  controls
                  src={`${process.env.PUBLIC_URL}/public/videos/editsavedsearch.mp4`}
                />
              </Col>
            </Row>
          </Col>
        </Row>
        <Row>
          <Col className="help-content">
            <h2>
              <FaVideo className="list-icon" />
              <span>Creating a Folder</span>
            </h2>
            <Row>
              <Col className="media-playback">
                <video
                  controls
                  src={`${process.env.PUBLIC_URL}/public/videos/creatingfolder.mp4`}
                />
              </Col>
            </Row>
          </Col>
        </Row>
        <Row>
          <Col className="help-content">
            <h2>
              <FaVideo className="list-icon" />
              <span>Create My Colleagues List and Share Content</span>
            </h2>
            <Row>
              <Col className="media-playback">
                <video controls src={`${process.env.PUBLIC_URL}/public/videos/sharecontent.mp4`} />
              </Col>
            </Row>
          </Col>
        </Row>
        <Row>
          <Col className="help-content">
            <h2>
              <FaVideo className="list-icon" />
              <span>Accessing Products</span>
            </h2>
            <Row>
              <Col className="media-playback">
                <video controls src={`${process.env.PUBLIC_URL}/public/videos/products.mp4`} />
              </Col>
            </Row>
          </Col>
        </Row>
      </PageSection>
    </styled.Help>
  );
};
