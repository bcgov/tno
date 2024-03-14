import { Action } from 'components/action';
import { FaRecycle } from 'react-icons/fa';
import { FaRightToBracket } from 'react-icons/fa6';
import { useProfileStore } from 'store/slices';
import { Show } from 'tno-core';

import { MenuButton } from './components';
import {
  ReportContentMenuOption,
  ReportMainMenuOption,
  ReportSettingsMenuOption,
} from './constants';
import { useReportEditContext } from './ReportEditContext';
import * as styled from './styled';

export interface IReportEditMenuProps {
  onChange?: (path: string) => void;
}

export const ReportEditMenu = ({ onChange }: IReportEditMenuProps) => {
  const { active, values } = useReportEditContext();
  const [, { storeReportOutput }] = useProfileStore();

  return (
    <styled.ReportEditMenu className="report-menu">
      <div className="report-main-menu">
        <div>
          <Action
            icon={<FaRightToBracket className="icon-exit" />}
            onClick={() => onChange?.(`/reports`)}
          />
        </div>
        <div className="report-name">{values.name ? values.name : 'New Report'}</div>
        <div>
          <MenuButton
            label="Settings"
            active={!active || active?.startsWith(ReportMainMenuOption.Settings)}
            onClick={() => onChange?.(`/reports/${values.id}/settings`)}
          />
        </div>
        <div>
          <MenuButton
            label="Content"
            active={active?.startsWith(ReportMainMenuOption.Content)}
            disabled={!values.id}
            onClick={() => onChange?.(`/reports/${values.id}/content`)}
          />
        </div>
        <div>
          <MenuButton
            label="Report Preview"
            active={active?.startsWith(ReportMainMenuOption.Preview)}
            disabled={!values.id}
            onClick={() => onChange?.(`/reports/${values.id}/preview`)}
          />
        </div>
      </div>
      <div className="report-secondary-menu">
        <Show visible={active?.startsWith(ReportMainMenuOption.Settings)}>
          <div>
            <MenuButton
              label="Template Design"
              active={active === ReportMainMenuOption.Settings}
              onClick={() => onChange?.(`/reports/${values.id}/settings`)}
            />
          </div>
          <div>
            <MenuButton
              label="Data Sources"
              active={active === ReportSettingsMenuOption.DataSources}
              onClick={() => onChange?.(`/reports/${values.id}/settings/sources`)}
            />
          </div>
          <div>
            <MenuButton
              label="Report Preferences"
              active={active === ReportSettingsMenuOption.Preferences}
              onClick={() => onChange?.(`/reports/${values.id}/settings/preferences`)}
            />
          </div>
          <div>
            <MenuButton
              label="Sending"
              active={active === ReportSettingsMenuOption.Send}
              onClick={() => onChange?.(`/reports/${values.id}/settings/send`)}
            />
          </div>
        </Show>
        {/* Content secondary menu */}
        <Show visible={active?.startsWith(ReportMainMenuOption.Content)}>
          <div>
            <MenuButton
              label="Curate Stories"
              active={active === ReportMainMenuOption.Content}
              onClick={() => onChange?.(`/reports/${values.id}/content`)}
            />
          </div>
          <div>
            <MenuButton
              label="Quick Sort"
              active={active === ReportContentMenuOption.Sort}
              onClick={() => onChange?.(`/reports/${values.id}/content/sort`)}
            />
          </div>
          <div>
            <MenuButton
              label="Executive Summary"
              active={active === ReportContentMenuOption.Summary}
              onClick={() => onChange?.(`/reports/${values.id}/content/summary`)}
            />
          </div>
        </Show>
        {/* Preview secondary menu */}
        <Show visible={active?.startsWith(ReportMainMenuOption.Preview)}>
          <div>
            <MenuButton
              label="View"
              active={active === ReportMainMenuOption.Preview}
              onClick={() => onChange?.(`/reports/${values.id}/preview`)}
            />
          </div>
          <div>
            <Action
              icon={<FaRecycle className="icon-green" />}
              label="Refresh Preview"
              onClick={() => storeReportOutput(undefined)}
            />
          </div>
        </Show>
      </div>
    </styled.ReportEditMenu>
  );
};
