import { Action } from 'components/action';
import { MenuButton } from 'components/button';
import { FaRecycle } from 'react-icons/fa';
import { FaCaretRight, FaRightToBracket } from 'react-icons/fa6';
import { useProfileStore } from 'store/slices';
import { Show } from 'tno-core';

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
          >
            <FaCaretRight className="caret" />
          </MenuButton>
        </div>
        <div>
          <MenuButton
            label="Content"
            active={active?.startsWith(ReportMainMenuOption.Content)}
            disabled={!values.id}
            onClick={() => onChange?.(`/reports/${values.id}/content`)}
          >
            <FaCaretRight className="caret" />
          </MenuButton>
        </div>
        <div>
          <MenuButton
            label="Report Preview"
            active={active?.startsWith(ReportMainMenuOption.View)}
            disabled={!values.id}
            onClick={() => onChange?.(`/reports/${values.id}/view`)}
          >
            <FaCaretRight className="caret" />
          </MenuButton>
        </div>
        <div>
          <MenuButton
            label="Send"
            active={active === ReportMainMenuOption.Send}
            onClick={() => onChange?.(`/reports/${values.id}/send`)}
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
            >
              <FaCaretRight className="caret" />
            </MenuButton>
          </div>
          <div>
            <MenuButton
              label="Data Sources"
              active={active === ReportSettingsMenuOption.DataSources}
              onClick={() => onChange?.(`/reports/${values.id}/settings/sources`)}
            >
              <FaCaretRight className="caret" />
            </MenuButton>
          </div>
          <div>
            <MenuButton
              label="Report Preferences"
              active={active === ReportSettingsMenuOption.Preferences}
              onClick={() => onChange?.(`/reports/${values.id}/settings/preferences`)}
            >
              <FaCaretRight className="caret" />
            </MenuButton>
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
            >
              <FaCaretRight className="caret" />
            </MenuButton>
          </div>
          <div>
            <MenuButton
              label="Quick Sort"
              active={active === ReportContentMenuOption.Sort}
              onClick={() => onChange?.(`/reports/${values.id}/content/sort`)}
            >
              <FaCaretRight className="caret" />
            </MenuButton>
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
        <Show visible={active?.startsWith(ReportMainMenuOption.View)}>
          <div>
            <MenuButton
              label="View"
              active={active === ReportMainMenuOption.View}
              onClick={() => onChange?.(`/reports/${values.id}/view`)}
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
        {/* Send secondary menu */}
        <Show visible={active?.startsWith(ReportMainMenuOption.Send)}>
          <div>
            <MenuButton
              label="Subscribers"
              active={active === ReportMainMenuOption.Send}
              onClick={() => onChange?.(`/reports/${values.id}/send`)}
            />
          </div>
        </Show>
      </div>
    </styled.ReportEditMenu>
  );
};
