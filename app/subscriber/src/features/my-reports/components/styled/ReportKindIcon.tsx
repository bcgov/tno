import styled from 'styled-components';
import { Row } from 'tno-core';

export const ReportKindIcon = styled(Row)`
  color: ${(props) => props.theme.css.iconPrimaryColor};
  .report-type {
    display: flex;
    font-size: 0.85rem;
    text-transform: uppercase;
    text-align: center;
    align-items: auto;
    border-radius: 0.25rem;
    padding: 0 0.35rem 0 0.35rem;
    color: white;
    max-width: fit-content;
    align-items: center;
  }

  .add-margin {
    margin-right: 0.5rem;
  }

  .manual {
    background-color: ${(props) => props.theme.css.reportManual};
  }

  .auto {
    background-color: ${(props) => props.theme.css.reportAuto};
  }

  .full-auto {
    background-color: ${(props) => props.theme.css.reportFullAuto};
  }

  .report-description {
    color: ${(props) => props.theme.css.fPrimaryColor};
  }
`;
