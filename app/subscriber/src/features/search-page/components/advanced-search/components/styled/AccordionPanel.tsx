import styled from 'styled-components';

export const AccordionPanel = styled.div`
  border-bottom: 1px solid ${(props) => props.theme.css.lineTertiaryColor};

  .accordion-header {
    display: flex;
    align-items: center;
    width: 100%;
    padding: 0.5em;
    background: transparent;
    border: none;
    cursor: pointer;
    font: inherit;
    font-weight: 650;
    color: ${(props) => props.theme.css.btnSecondaryColor};
    text-align: left;

    svg {
      margin-right: 0.5em;
      align-self: center;
    }

    .accordion-title {
      flex: 1;
    }

    .active-filter-icon {
      color: ${(props) => props.theme.css.iconGreen};
    }

    .drop-icon {
      margin-right: 0;
    }
  }

  .accordion-body {
    padding: 0 0 0.5em;
  }
`;
