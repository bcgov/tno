import {
  FaClipboard,
  FaCog,
  FaCogs,
  FaComment,
  FaEnvelope,
  FaFire,
  FaFolder,
  FaHome,
  FaMoon,
  FaNewspaper,
  FaSearch,
  FaSlidersH,
  FaUsers,
  FaUserTie,
} from 'react-icons/fa';

export interface ISideBarMenuItems {
  [key: string]: {
    label: string;
    path: string;
    icon: JSX.Element;
    secondaryIcon?: JSX.Element;
    secondaryIconRoute?: string;
  };
}

/** The below manages the items that will appear in the left navigation bar in sequential order. */
export const SidebarMenuItems: ISideBarMenuItems = {
  home: {
    label: 'Home',
    path: 'home',
    icon: <FaHome />,
  },
  topStories: {
    label: 'Top Stories',
    path: 'topstories',
    icon: <FaFire />,
  },
  todaysCommentary: {
    label: "Today's Commentary",
    path: 'todayscommentary',
    icon: <FaComment />,
  },
  todaysFrontPages: {
    label: "Today's Front Pages",
    path: 'todaysfrontpages',
    icon: <FaNewspaper />,
  },
  pressGallery: {
    label: 'Press Gallery',
    path: 'pressgallery',
    icon: <FaUsers />,
  },
  eveningOverview: {
    label: 'Evening Overview',
    path: 'eveningoverview',
    icon: <FaMoon />,
  },
  filterMedia: {
    label: 'Filter by media type',
    path: 'filtermedia',
    icon: <FaSlidersH />,
  },
  myProducts: {
    label: 'MMI Products',
    path: 'myproducts',
    icon: <FaEnvelope />,
  },
  myMinister: {
    label: 'My Minister',
    path: 'myminister',
    icon: <FaUserTie />,
    secondaryIcon: <FaCog />,
    secondaryIconRoute: '/landing/settings',
  },
  mySearches: {
    label: 'My Saved Searches',
    path: 'mysearches',
    icon: <FaSearch />,
  },
  folders: {
    label: 'My Folders',
    path: 'folders',
    icon: <FaFolder />,
  },
  myReports: {
    label: 'My Reports',
    path: 'myreports',
    icon: <FaClipboard />,
  },
  settings: {
    label: 'Settings',
    path: 'settings',
    icon: <FaCogs />,
  },
};

export const sidebarMenuItemsArray = Object.keys(SidebarMenuItems).map(
  (key) => SidebarMenuItems[key],
);
