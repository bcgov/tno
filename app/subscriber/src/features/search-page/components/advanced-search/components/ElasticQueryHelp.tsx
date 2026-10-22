import React from 'react';
import { createPortal } from 'react-dom';
import Draggable from 'react-draggable';
import { FaInfoCircle } from 'react-icons/fa';
import { FaX } from 'react-icons/fa6';
import { Col, Row, Show } from 'tno-core';

import * as styled from './styled';

export interface IElasticQueryHelpProps {
  queryType?: 'simple-query-string' | 'query-string';
}

/** Component that displays help text for elastic search queries */
export const ElasticQueryHelp: React.FC<IElasticQueryHelpProps> = ({
  queryType = 'simple-query-string',
}) => {
  const [show, setShow] = React.useState(false);
  const [position, setPosition] = React.useState<{ top: number; left: number }>();
  const iconRef = React.useRef<HTMLSpanElement>(null);
  const nodeRef = React.useRef<HTMLDivElement>(null);

  const handleInfoClick = () => {
    if (!show && iconRef.current) {
      const rect = iconRef.current.getBoundingClientRect();
      setPosition({ top: rect.bottom + 4, left: rect.left });
    }
    setShow(!show);
  };

  // The help opens beside its icon, which sits at the sidebar's edge; keep it inside the viewport.
  React.useLayoutEffect(() => {
    if (!show || !position || !nodeRef.current) return;
    const width = nodeRef.current.offsetWidth;
    const left = Math.max(8, Math.min(position.left, window.innerWidth - width - 8));
    if (left !== position.left) setPosition({ ...position, left });
  }, [show, position]);

  return (
    <styled.ElasticInfo className="elastic-info">
      <span ref={iconRef}>
        <FaInfoCircle onClick={handleInfoClick} className="info-icon" />
      </span>
      {show &&
        position &&
        // Rendered on the body so the sidebar's overflow cannot clip it.
        createPortal(
          <styled.ElasticInfo className="elastic-info">
            <Draggable nodeRef={nodeRef}>
              <div
                ref={nodeRef}
                style={{ position: 'fixed', top: position.top, left: position.left }}
              >
                <Row>
                  <FaX onClick={() => setShow(false)} className="close-icon" />
                </Row>
                <Show visible={queryType === 'simple-query-string'}>
                  <Col>
                    The keywords query supports the following operators:
                    <ul>
                      <Row className="row">
                        <code>+</code> signifies AND operation
                      </Row>
                      <Row className="row">
                        <code>|</code> signifies OR operation
                      </Row>
                      <Row className="row">
                        <code>-</code> negates a single token
                      </Row>
                      <Row className="row">
                        <code>"</code> wraps a number of tokens to signify a phrase for searching
                      </Row>
                      <Row className="row">
                        <code>*</code> at the end of a term signifies a prefix query
                      </Row>
                      <Row className="row">
                        <code>(</code> and <code>)</code> signify precedence
                      </Row>
                      <Row className="row">
                        <code>~N</code> after a word signifies edit distance (fuzziness)
                      </Row>
                      <Row className="row">
                        <code>~N</code> after a phrase signifies slop amount
                      </Row>
                    </ul>
                  </Col>
                </Show>
                <Show visible={queryType === 'query-string'}>
                  <Col>
                    The keywords query supports the following operators:
                    <ul>
                      <Row className="row">
                        <code>AND</code> <code>&&</code> <code>+</code> signifies AND operation
                      </Row>
                      <Row className="row">
                        <code>OR</code> <code>||</code> signifies OR operation
                      </Row>
                      <Row className="row">
                        <code>NOT</code> <code>!</code> <code>-</code> negates a single token
                      </Row>
                      <Row className="row">
                        <code>"</code> wraps a number of tokens to signify a phrase for searching
                      </Row>
                      <Row className="row">
                        <code>*</code> wildcard characters
                      </Row>
                      <Row className="row">
                        <code>?</code> wildcard character
                      </Row>
                      <Row className="row">
                        <code>(</code> and <code>)</code> signify precedence
                      </Row>
                      <Row className="row">
                        <code>~N</code> after a word signifies edit distance (fuzziness)
                      </Row>
                      <Row className="row">
                        <code>~N</code> after a phrase signifies slop amount
                      </Row>
                    </ul>
                  </Col>
                </Show>
              </div>
            </Draggable>
          </styled.ElasticInfo>,
          document.body,
        )}
    </styled.ElasticInfo>
  );
};
