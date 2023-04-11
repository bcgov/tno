import * as styled from './styled';

interface IHeaderProps extends React.HTMLAttributes<HTMLDivElement> {
  /**
   * The site name.
   */
  name: string;
}

/**
 * Provides a header element.
 * @param param0 Header element attributes.
 * @returns Header component.
 */
export const Header: React.FC<IHeaderProps> = ({ name, children, ...rest }) => {
  return (
    <styled.Header {...rest}>
      <div>
        <a href={`${process.env.PUBLIC_URL}/`}>
          <img alt="BC Gov logo" src={process.env.PUBLIC_URL + '/assets/gov_bc_logo.svg'} />
        </a>
      </div>
      <div className="vl" />
      <a href={`${process.env.PUBLIC_URL}/`}>
        <img
          alt="TNO logo"
          className="app-logo"
          src={process.env.PUBLIC_URL + '/assets/mminsights_logo.svg'}
        />
      </a>
      <div>
        <div className="title" />
        <div className="options">{children}</div>
      </div>
    </styled.Header>
  );
};
