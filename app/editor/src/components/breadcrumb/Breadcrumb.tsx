import React from 'react';
import { FaFolder } from 'react-icons/fa';
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
  const [, setSearchParams] = useSearchParams();

  React.useEffect(() => {
    setSearchParams({ path: path });
  }, [path, setSearchParams]);

  /** split path into array in order to display breadcrumb as individual items */
  let splitPath = path.split('/');
  const checkPath = (index: number, item: string) => {
    if (index === 0) setPath(`${item}`);
    /** last item not clickable */
    if (index !== splitPath.length - 1) setPath(splitPath.slice(0, index + 1).join('/'));
  };

  return (
    <styled.Breadcrumb>
      <p>Directory:</p>
      <FaFolder
        className={splitPath.length === 1 ? 'current' : 'clickable'}
        onClick={() => checkPath(0, '')}
      />
      {splitPath.map((name: string, index: number) => (
        <p
          key={`${name}-${index}`}
          className={index !== splitPath.length - 1 ? 'clickable' : 'current'}
          onClick={() => checkPath(index, name)}
        >
          {name}
        </p>
      ))}
    </styled.Breadcrumb>
  );
};
