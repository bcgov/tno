import { Col } from 'tno-core';

export interface IElasticQueryHelpProps {
  queryType?: 'simple-query-string' | 'query-string';
}

export const ElasticQueryHelp: React.FC<IElasticQueryHelpProps> = ({ queryType }) => {
  if (queryType === 'simple-query-string')
    return (
      <Col>
        <div>
          The keywords query supports the following operators:
          <ul>
            <li>
              <code>+</code> signifies AND operation
            </li>
            <li>
              <code>|</code> signifies OR operation
            </li>
            <li>
              <code>-</code> negates a single token
            </li>
            <li>
              <code>"</code> wraps a number of tokens to signify a phrase for searching
            </li>
            <li>
              <code>*</code> at the end of a term signifies a prefix query
            </li>
            <li>
              <code>(</code> and <code>)</code> signify precedence
            </li>
            <li>
              <code>~N</code> after a word signifies edit distance (fuzziness)
            </li>
            <li>
              <code>~N</code> after a phrase signifies slop amount
            </li>
          </ul>
        </div>
      </Col>
    );

  if (queryType === 'query-string')
    return (
      <Col>
        <div>
          The keywords query supports the following operators:
          <ul>
            <li>
              <code>AND</code> <code>+</code> signifies AND operation
            </li>
            <li>
              <code>OR</code> <code>||</code> signifies OR operation
            </li>
            <li>
              <code>NOT</code> <code>-</code> negates a single token
            </li>
            <li>
              <code>"</code> wraps a number of tokens to signify a phrase for searching
            </li>
            <li>
              <code>*</code> wildcard characters
            </li>
            <li>
              <code>?</code> wildcard character
            </li>
            <li>
              <code>(</code> and <code>)</code> signify precedence
            </li>
            <li>
              <code>~N</code> after a word signifies edit distance (fuzziness)
            </li>
            <li>
              <code>~N</code> after a phrase signifies slop amount
            </li>
          </ul>
        </div>
      </Col>
    );

  return null;
};
