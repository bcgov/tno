export const formatDate = (date: string) => {
  const d = new Date(date);
  const day = d.toLocaleDateString('en-us', { day: '2-digit' });
  const month = d.toLocaleDateString('en-us', { month: 'short' }).toUpperCase();
  const year = d.toLocaleDateString('en-us', { year: 'numeric' });
  const hour = new Intl.DateTimeFormat('en-us', {
    hour12: false,
    hour: '2-digit',
    minute: 'numeric',
  }).format(d);
  return `${day}-${month}-${year}, ${hour}`;
};
