import { toast } from 'react-toastify';

export const handleCopyKeyWords = (event: any, cell: any) => {
  navigator.clipboard.writeText(cell.original.settings!.search);
  // animate the clipboar icon to show something happened
  event.target.classList.toggle('animate');
  setTimeout(() => {
    event.target.classList.toggle('animate');
  }, 200);
  toast.success('Keywords copied to clipboard');
};
