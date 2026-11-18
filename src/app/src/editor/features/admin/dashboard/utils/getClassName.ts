export const getClassName = (status: string) => {
  switch (status) {
    case 'Running':
      return 'running';
    case 'Not Running':
      return 'warning';
    case 'Failed':
      return 'error';
    case 'Disabled':
      return 'disabled';
    case 'Never Run':
    default:
      return '';
  }
};
