import { TooltipMenu } from 'components/tooltip-menu';
import { FaInfoCircle } from 'react-icons/fa';
import { Col, Row, Show } from 'tno-core';

import * as styled from './styled';

export interface IElasticQueryHelpProps {
  queryType?: 'simple-query-string' | 'query-string';
}

/** Component that displays help text for elastic search queries */
export const ElasticQueryHelp: React.FC<IElasticQueryHelpProps> = ({ queryType }) => {
  if (!queryType) return null;

  return (
    <styled.ElasticInfo className="elastic-info">
      <FaInfoCircle data-tooltip-id="elastic-info" className="info-icon" />
      <TooltipMenu openOnClick id="elastic-info">
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
                <code>AND</code> <code>+</code> signifies AND operation
              </Row>
              <Row className="row">
                <code>OR</code> <code>||</code> signifies OR operation
              </Row>
              <Row className="row">
                <code>NOT</code> <code>-</code> negates a single token
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
      </TooltipMenu>
    </styled.ElasticInfo>
  );
};
