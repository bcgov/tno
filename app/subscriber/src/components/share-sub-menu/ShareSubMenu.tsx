import React from 'react';
import { FaShare } from 'react-icons/fa';
import { toast } from 'react-toastify';
import { useColleagues, useLookup } from 'store/hooks';
import {
  IContentModel,
  IOptionItem,
  Modal,
  OptionItem,
  Row,
  Select,
  Settings,
  useModal,
} from 'tno-core';

import * as styled from './styled';

export interface IShareSubMenuProps {
  /**
   * The content that has been selected to add to a folder
   */
  selectedContent: IContentModel[];
}

/** Component that renders the button that gives users access to a sub menu that will allow them to add selected content to
 * an existing folder. Or create a new one.
 */
export const ShareSubMenu: React.FC<IShareSubMenuProps> = ({ selectedContent }) => {
  const [options, setOptions] = React.useState<IOptionItem[]>([]);
  const [email, setEmail] = React.useState<string>();
  const [{ settings }] = useLookup();
  const { toggle, isShowing } = useModal();
  const [{ getColleagues, sendNotification }] = useColleagues();

  React.useEffect(() => {
    if (!options.length) {
      getColleagues().then((data) => {
        setOptions(
          data
            .filter((i) => i.colleague !== undefined)
            .map((d) => {
              return new OptionItem(`${d.colleague?.email}`, d.colleague?.email);
            }),
        );
      });
    }
  }, [options.length, getColleagues]);

  const handleSend = async () => {
    try {
      const notificationId = settings.find((s) => s.name === Settings.DefaultAlert)?.value;
      if (notificationId) {
        selectedContent.forEach(async (c) => {
          if (email) {
            await sendNotification(parseInt(notificationId), email, c.id);
          }
        });
        toast.success('Notification has been successfully requested');
      } else {
        toast.error(`${Settings.DefaultAlert} setting needs to be configured.`);
      }
    } catch {}
  };

  const message = `Share ${selectedContent.length} selected content${
    selectedContent.length > 1 ? 's' : ''
  } with colleague ?`;

  const selectColleague = (
    <Select
      label="Select Colleague"
      name="colleagues"
      options={options}
      value={options.find((o) => o?.value === email) ?? ''}
      isClearable={false}
      onChange={(newValue) => {
        const option = newValue as OptionItem;
        setEmail(`${option.value}`);
      }}
    />
  );

  return (
    <styled.ShareSubMenu className="share-sub-menu">
      <Row justifyContent="end">
        <FaShare className="share-story" data-tooltip-id="share-story" onClick={() => toggle()} />
      </Row>
      <Modal
        headerText="Share Content"
        body={message}
        component={selectColleague}
        isShowing={isShowing}
        hide={toggle}
        type="default"
        confirmText="Share"
        enableConfirm={selectedContent.length > 0 && email !== undefined}
        onConfirm={() => {
          handleSend();
          toggle();
        }}
      />
    </styled.ShareSubMenu>
  );
};
