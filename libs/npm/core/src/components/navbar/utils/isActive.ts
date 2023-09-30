export const isActive = (
  path: string,
  match?: string,
  exact: boolean = true,
  activeHover?: string,
  uid?: string,
) => {
  if (path === match && activeHover === '') return true;
  if (exact && activeHover === '') return path === match;
  if (!path || !match) return false;
  if (match.startsWith('/') && activeHover === '') return path.startsWith(match);
  if (activeHover === uid?.toLowerCase()) return true;
  if (activeHover !== uid?.toLowerCase() && !!activeHover) return false;

  return path.endsWith(match);
};
