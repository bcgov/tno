import { toast } from 'react-toastify';

export const handleCopyKeyWords = (event: any, keywords: any) => {
  navigator.clipboard.writeText(keywords);
  // animate the clipboard icon to show something happened
  event.target.classList.toggle('animate');
  setTimeout(() => {
    event.target.classList.toggle('animate');
  }, 200);
  toast.success('Keywords copied to clipboard');
};
