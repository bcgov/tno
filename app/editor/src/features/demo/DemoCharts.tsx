import { CategoryScale } from 'chart.js';
import Chart from 'chart.js/auto';
import React from 'react';
import { Bar } from 'react-chartjs-2';
import { ChartJSOrUndefined } from 'react-chartjs-2/dist/types';
import { Button } from 'tno-core';

Chart.register(CategoryScale);

const rawData = [
  { year: 2010, count: 10 },
  { year: 2011, count: 20 },
  { year: 2012, count: 14 },
  { year: 2013, count: 30 },
  { year: 2014, count: 120 },
  { year: 2015, count: 67 },
  { year: 2016, count: 34 },
];

const data = {
  labels: rawData.map((row) => row.year),
  datasets: [
    {
      label: 'By Year',
      data: rawData.map((row) => row.count),
    },
  ],
};

export const DemoCharts = () => {
  const elementRef = React.useRef<ChartJSOrUndefined<'bar', number[], number>>(null);
  const imageRef = React.useRef<HTMLImageElement>(null);

  const [ready, setReady] = React.useState(false);

  const download = () => {
    const element = elementRef.current;
    const image = element?.toBase64Image();
    if (imageRef.current) imageRef.current.src = image ?? '';
  };

  return (
    <div>
      <Bar
        ref={elementRef}
        data={data}
        options={{
          animation: {
            onComplete: () => {
              setReady(true);
            },
          },
        }}
      />
      <Button onClick={download} disabled={!ready}>
        PNG
      </Button>
      <div
        onClick={() => {
          if (imageRef.current) imageRef.current.src = '';
        }}
      >
        <img ref={imageRef} alt="" src="" />
      </div>
    </div>
  );
};
