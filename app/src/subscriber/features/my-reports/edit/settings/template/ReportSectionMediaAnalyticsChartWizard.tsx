import { Button } from 'components/button';
import React from 'react';
import { ErrorBoundary } from 'react-error-boundary';
import { FaCaretRight } from 'react-icons/fa';
import { FaCaretLeft } from 'react-icons/fa6';
import { Row, Show } from 'tno-core';

import {
  ChartPicker,
  ChartViewer,
  ConfigureColours,
  ConfigureDataset,
  ConfigureLabels,
  ConfigureLegend,
  ConfigureScale,
  IChartSectionProps,
} from './charts';
import { wizardMenuOptions } from './constants';
import * as styled from './styled';
import { IChartData } from './utils';

export interface IReportSectionMediaAnalyticsChartWizardProps extends IChartSectionProps {
  data?: IChartData;
  onDisableDrag?: (disable: boolean) => void;
}

/**
 * Provides a way to configure the dataset for the chart with a wizard.
 * @param param0 Component properties.
 * @returns Component.
 */
export const ReportSectionMediaAnalyticsChartWizard = React.forwardRef<
  HTMLDivElement,
  IReportSectionMediaAnalyticsChartWizardProps
>(({ sectionIndex, chartIndex, data, onDisableDrag, ...rest }, ref) => {
  const [active, setActive] = React.useState(wizardMenuOptions[0].id);

  const currentIndex = wizardMenuOptions.findIndex((o) => o.id === active);

  return (
    <styled.ChartWizard>
      <div className="chart-config">
        <div>
          <ol>
            {wizardMenuOptions.map((item) => {
              return (
                <li
                  key={item.id}
                  className={active === item.id ? 'active' : ''}
                  onClick={() => setActive(item.id)}
                >
                  {item.label}
                </li>
              );
            })}
          </ol>
        </div>
        <div>
          <Show visible={active === 'chartType'}>
            <ChartPicker sectionIndex={sectionIndex} chartIndex={chartIndex} />
          </Show>
          <Show visible={active === 'dataset'}>
            <ConfigureDataset sectionIndex={sectionIndex} chartIndex={chartIndex} />
          </Show>
          <Show visible={active === 'scale'}>
            <ConfigureScale sectionIndex={sectionIndex} chartIndex={chartIndex} />
          </Show>
          <Show visible={active === 'labels'}>
            <ConfigureLabels
              sectionIndex={sectionIndex}
              chartIndex={chartIndex}
              onDisableDrag={onDisableDrag}
            />
          </Show>
          <Show visible={active === 'legend'}>
            <ConfigureLegend sectionIndex={sectionIndex} chartIndex={chartIndex} />
          </Show>
          <Show visible={active === 'colours'}>
            <ConfigureColours
              sectionIndex={sectionIndex}
              chartIndex={chartIndex}
              onDisableDrag={onDisableDrag}
            />
          </Show>
          <Row justifyContent="space-between">
            <Show visible={currentIndex > 0}>
              <Button
                variant="secondary"
                onClick={() => {
                  setActive(wizardMenuOptions[currentIndex - 1].id);
                }}
              >
                <FaCaretLeft />
                Previous
              </Button>
            </Show>
            <Show visible={currentIndex < wizardMenuOptions.length - 1}>
              <Button
                variant="secondary"
                onClick={() => {
                  setActive(wizardMenuOptions[currentIndex + 1].id);
                }}
              >
                Next
                <FaCaretRight />
              </Button>
            </Show>
          </Row>
        </div>
      </div>
      <ErrorBoundary
        fallback={
          <div>
            <p>An unexpected error has occurred.</p>
            <p>You will need to close and reopen this chart.</p>
          </div>
        }
      >
        <ChartViewer sectionIndex={sectionIndex} chartIndex={chartIndex} data={data} />
      </ErrorBoundary>
    </styled.ChartWizard>
  );
});
