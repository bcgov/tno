import { useNavigate } from 'react-router-dom';

export const UseNavigateAndScroll = (path: string) => {
  const navigate = useNavigate();
  navigate(path);
  const cont = document.getElementsByClassName('contents-container')[0];
  if (cont) {
    cont.scrollTo(0, 0);
  }
};
