import { TooltipMenu } from 'components/tooltip-menu';
import { FaInfoCircle } from 'react-icons/fa';
import { Col } from 'tno-core';

import * as styled from './styled';

/** Component that displays help text for elastic search queries */
export const InfoAdvancedQueryOperators: React.FC = () => {
  return (
    <styled.InfoAdvancedQueryOperators>
      <FaInfoCircle data-tooltip-id="toggle-info-advanced-query-operator" className="info-icon" />
      <TooltipMenu openOnClick id="toggle-info-advanced-query-operator">
        <Col>
          <div>
            <FaInfoCircle className="info-icon" />
            <span>
              <b>Advanced query operators</b>
            </span>
            <table>
              <thead>
                <tr>
                  <td className="col-word-wrap">
                    When this toggle is enabled, the Search input will accept some additional
                    operators for composing advanced queries.
                  </td>
                </tr>
              </thead>
              <tbody>
                <tr>
                  <td>
                    <table>
                      <thead className="table-header">
                        <tr>
                          <td valign="middle" className="col-basic">
                            <span className="tooltip-fonts">BASIC OPERATORS</span>
                          </td>
                          <td valign="middle" className="col-advanced">
                            <span className="tooltip-fonts">ADVANCED OPERATORS</span>
                          </td>
                          <td valign="middle" className="col-usage">
                            <span className="tooltip-fonts">USAGE</span>
                          </td>
                          <td valign="middle" className="col-example">
                            <span className="tooltip-fonts">EXAMPLE</span>
                          </td>
                        </tr>
                      </thead>
                      <tbody>
                        <tr>
                          <td valign="top">
                            <span className="tooltip-fonts">+</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">AND && +</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">AND operation</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">
                              cat AND dog<br></br>will only return results that contain <i>both</i>{' '}
                              terms
                            </span>
                          </td>
                        </tr>
                        <tr>
                          <td valign="top">
                            <span className="tooltip-fonts">|</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">OR |</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">OR operation</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">
                              cat | dog<br></br>will return results that contain <i>either</i> cat
                              OR dog
                            </span>
                          </td>
                        </tr>
                        <tr>
                          <td valign="top">
                            <span className="tooltip-fonts">-</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">NOT ! -</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">negates a single token</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">
                              dog -cat<br></br>results will contain dog, but not if the result
                              contains CAT
                            </span>
                          </td>
                        </tr>
                        <tr>
                          <td valign="top">
                            <span className="tooltip-fonts">"</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">"</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">
                              wraps a number of tokens to signify a phrase
                            </span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">
                              "dog and cat adoption"<br></br>results must contain this entire phrase
                            </span>
                          </td>
                        </tr>
                        <tr>
                          <td valign="top">
                            <span className="tooltip-fonts">*</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">? *</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">
                              used at the end of a term to perform a prefix query
                            </span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">
                              adopt*<br></br>will return results that contain words like{' '}
                              <i>adoption, adopting, adoptable,</i> any words that begin with the
                              word
                            </span>
                            adopt
                          </td>
                        </tr>
                        <tr>
                          <td valign="top">
                            <span className="tooltip-fonts">( and )</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">( and )</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">declares precedence</span>
                          </td>
                          <td></td>
                        </tr>
                        <tr>
                          <td valign="top">
                            <span className="tooltip-fonts">~N</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">~N</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">
                              after a word: declares edit distance (fuzziness of search)
                            </span>
                          </td>
                          <td></td>
                        </tr>
                        <tr>
                          <td valign="top">
                            <span className="tooltip-fonts">~N</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">~N</span>
                          </td>
                          <td valign="top">
                            <span className="tooltip-fonts">
                              after a phrase: signifies slop amount
                            </span>
                          </td>
                          <td></td>
                        </tr>
                      </tbody>
                    </table>
                  </td>
                </tr>
              </tbody>
            </table>
          </div>
        </Col>
      </TooltipMenu>
    </styled.InfoAdvancedQueryOperators>
  );
};
