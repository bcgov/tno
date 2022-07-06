export const isActive = (
  path: string,
  match?: string,
  exact: boolean = true,
  activeHover?: string,
  label?: string,
) => {
  if (path === match && activeHover === '') return true;
  if (exact && activeHover === '') return path === match;
  if (!path || !match) return false;
  if (match.startsWith('/') && activeHover === '') return path.startsWith(match);
  if (activeHover === label?.toLowerCase()) return true;

  return path.endsWith(match);
};
