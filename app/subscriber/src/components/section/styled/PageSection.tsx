import styled from 'styled-components';

export const PageSection = styled.div<{ $ignoreMinWidth?: boolean; $ignoreLastChildGap?: boolean }>`
  display: flex;
  flex-direction: column;
  background: ${(props) => props.theme.css.bkSecondary};
  box-shadow: ${(props) => props.theme.css.boxShadow};
  align-self: stretch;
  min-width: ${(props) => (props.$ignoreMinWidth ? 'unset' : 'fit-content')};

  .back-button {
    margin-right: 1rem;
    &:hover {
      cursor: pointer;
    }
  }

  .content-headline {
    margin-bottom: 0.25rem;
  }

  @media (min-width: 500px) {
    margin: 0 0.75rem 0.75rem 0.75rem;
  }

  .page-section-title {
    margin: 1rem;
    color: ${(props) => props.theme.css.hPrimaryColor};
    font-size: 26px;
    font-weight: 600;
    line-height: 32.68px;
    border-bottom: solid 1px ${(props) => props.theme.css.linePrimaryColor};
    margin-bottom: 0rem;
  }
  .page-section-icon {
    padding-right: 1rem;
  }

  .page-icon {
    margin: 0.1em 0.25em 0 0;
    color: ${(props) => props.theme.css.iconPurpleColor};
    // size of font and icons
    max-height: 26px;
  }

  .page-content {
    display: flex;
    flex-direction: column;
    padding: 1rem;
    gap: 0.25rem;
  }

  ${(props) =>
    !props.$ignoreLastChildGap &&
    `
  > div:last-child {
    @extend .page-content;
  }

  `}
`;
