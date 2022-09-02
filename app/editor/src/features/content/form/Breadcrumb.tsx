import { useEffect } from 'react';
import { useSearchParams } from 'react-router-dom';

import * as styled from './styled';
export interface IBreadcrumbProps {
  /** the current path being viewed in the clip explorer */
  path: string;
  /** set path of the parent component */
  setPath: (path: string) => void;
}

/** component used to navigate state controlled path directory, also updates the
 *  search params to the current directory
 */
export const Breadcrumb: React.FC<IBreadcrumbProps> = ({ path, setPath }) => {
  const [searchParams, setSearchParams] = useSearchParams();
  /** split path into array in order to display breadcrumb as individual items */
  let splitPath = path.split('/').filter((s) => !!s);
  const checkPath = (index: number, item: string) => {
    if (index === 0) setPath(`${item}`);
    /** last item not clickeable */
    if (index !== splitPath.length - 1) setPath(splitPath.slice(0, index + 1).join('/'));
  };
  useEffect(() => {
    setSearchParams({ path: path });
  }, [path, setSearchParams]);
  return (
    <styled.Breadcrumb>
      {splitPath.map((i: string, index: number) => (
        <p
          className={index !== splitPath.length - 1 ? 'clickeable' : 'current'}
          onClick={() => checkPath(index, i)}
        >
          {i}
        </p>
      ))}
    </styled.Breadcrumb>
  );
};
