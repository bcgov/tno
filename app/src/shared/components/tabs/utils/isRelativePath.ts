export const isRelativePath = (path?: string) => (!!path ? !path?.startsWith('/') : false);
