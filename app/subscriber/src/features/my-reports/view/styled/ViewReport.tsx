import styled from 'styled-components';
import { Col } from 'tno-core';

export const ViewReport = styled(Col)`
  .container {
    .header {
      border-bottom: 1px solid ${(props) => props.theme.css.bsGray500};
    }
    .title {
      color: ${(props) => props.theme.css.redHeadingColor};
      align-self: center;
    }
    .view-report {
      padding: 0.5em;
    }
    .back-icon {
      margin-top: 0.35em;
      margin-right: 0.5em;
      &:hover {
        color: ${(props) => props.theme.css.subscriberPurple};
        cursor: pointer;
        transform: scale(1, 1.1);
      }
    }
    background-color: white;
    padding: 0.5em;
    margin-bottom: 0.25em;
    margin-left: 0.25em;
    margin-right: 0.25em;
    box-shadow: 0 0 3px 0 rgba(0, 0, 0, 0.2);
    border-radius: 0.75em;
    width: 98.5%;
  }
`;
