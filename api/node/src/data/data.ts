const rawData = [
  { year: 2010, count: 10 },
  { year: 2011, count: 20 },
  { year: 2012, count: 14 },
  { year: 2013, count: 30 },
  { year: 2014, count: 120 },
  { year: 2015, count: 67 },
  { year: 2016, count: 34 },
];

export const data = {
  labels: rawData.map((row) => row.year),
  datasets: [
    {
      label: 'By Year',
      data: rawData.map((row) => row.count),
    },
  ],
};
