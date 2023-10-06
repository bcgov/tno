export const navigateAndScroll = (navigate: any, path: string) => {
  navigate(path);
  const cont = document.getElementsByClassName('contents-container')[0];
  if (cont) {
    cont.scrollTo(0, 0);
  }
};
