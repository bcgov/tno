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

export interface INavbarOptions {
  [key: string]: {
    label: string;
    path: string;
    icon: JSX.Element;
    groupName?: string;
    secondaryIcon?: JSX.Element;
    secondaryIconRoute?: string;
  };
}

export const NavbarOptions: INavbarOptions = {
  home: {
    label: 'Featured Stories',
    path: 'landing/home',
    icon: <FaHome />,
  },
  topStories: {
    label: 'Top Stories',
    path: 'landing/topstories',
    icon: <FaFire />,
  },
  todaysCommentary: {
    label: "Today's Commentary",
    path: 'landing/todayscommentary',
    icon: <FaComment />,
  },
  todaysFrontPages: {
    label: "Today's Front Pages",
    path: 'landing/todaysfrontpages',
    icon: <FaNewspaper />,
  },
  pressGallery: {
    label: 'Press Gallery',
    path: 'landing/pressgallery',
    icon: <FaUsers />,
  },
  eveningOverview: {
    label: 'Evening Overview',
    path: 'landing/eveningoverview',
    icon: <FaMoon />,
  },
  filterMedia: {
    label: 'Filter by media type',
    path: 'landing/filtermedia',
    icon: <FaSlidersH />,
  },
  myProducts: {
    label: 'MMI Products',
    path: 'products',
    icon: <FaEnvelope />,
  },
  myMinister: {
    label: 'My Minister',
    path: 'landing/myminister',
    icon: <FaUserTie />,
    secondaryIcon: <FaCog />,
    groupName: 'MY CONTENT',
    secondaryIconRoute: '/landing/settings',
  },
  mySearches: {
    groupName: 'MY CONTENT',
    label: 'My Saved Searches',
    path: 'landing/mysearches',
    icon: <FaSearch />,
  },
  folders: {
    groupName: 'MY CONTENT',
    label: 'My Folders',
    path: 'folders',
    icon: <FaFolder />,
  },
  myReports: {
    groupName: 'MY CONTENT',
    label: 'My Reports',
    path: 'reports',
    icon: <FaClipboard />,
  },
  settings: {
    groupName: 'USER RESOURCES',
    label: 'Settings',
    path: 'landing/settings',
    icon: <FaCogs />,
  },
};

export const navbarOptions = Object.keys(NavbarOptions).map((key) => NavbarOptions[key]);
