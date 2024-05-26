import { createContext, useContext, useEffect, useState } from "react";
import { useLocation, useNavigate } from "react-router-dom";
import {
  AppLayoutProps,
  SideNavigationProps,
} from "@cloudscape-design/components";
import { useAuth } from "../AuthProvider/AuthProvider";
import { useApi } from "../../Hooks/apiHook";

interface MenuContextType {
  appMenuItems: AppMenuItem[];
  sideNavMenuItems: SideNavigationProps.Item[];
  onMenuClick: (event: number) => Promise<void>;
  currentContentType: AppLayoutProps.ContentType;
  onHomeClick: () => Promise<void>;
  currentMenu: MenuDetail;
}

const MenuContext = createContext<MenuContextType>(null);

export interface MenuItem {
  Name: string;
  ParentMenu: null;
  SubMenus: MenuItem[] | null;
  Event: number | null;
  Position: number;
  Id: string;
  CanDelete: boolean;
}

export interface AppMenuItem {
  href: string;
  path: string;
  name: string;
  parentMenu: any;
  subMenus: AppMenuItem[] | null;
  event: number | null;
  position: number;
  id: string;
  canDelete: boolean;
}

export enum ColumnType {
  String = 0,
  Boolean = 1,
  Button = 2,
  Link = 3,
  Hidden = 4,
  Date = 5,
  CheckBox = 6,
}
enum ColumnDisplay {
  Show = 0,
  Hide = 1,
}

enum ConditionComparison {
  Equals = 0,
  NotEquals = 1,
  Contains = 2,
  IsNotNull = 3,
  IsNull = 4,
  GreaterThan = 5,
  GreaterThanOrEqual = 6,
  LessThan = 7,
  LessThanOrEqual = 8,
}
interface ColumnCondition {
  ColumnName: string;
  Comparison: ConditionComparison;
  ColumnValue: string;
}

interface ColumnSetting {
  ColumnSettingType: number;
  Display?: ColumnDisplay;
  Conditions?: ColumnCondition[];
}

export interface ViewColumn {
  ColumnSpan: number;
  ColumnLabel: string;
  ColumnName: string;
  ColumnType: ColumnType;
  ColumnSetting: ColumnSetting;
  LinkLabel?: string;
}

interface MenuDetail {
  Description?: string;
  Title?: string;
  AllowInMenu?: boolean;
  DataForGettingMenu?: any;
  ActionType?: number;
  Pages?: 0;
  LinesPerPage?: number;
  CurrentPage?: number;
  TotalLines?: number;
  Filter?: string;
  RequiresAuthorization?: boolean;
  EventParameters?: any;
  Id?: number;
  Parameters?: any;
  Columns?: ViewColumn[];
  ViewData?: any[];
}

const MapMenuItemsToSideNavItems = (
  items: MenuItem[],
  root: boolean = true
): SideNavigationProps.Item[] => {
  const results: SideNavigationProps.Item[] = [];

  if (root) {
    results.push({ text: "Home", href: "#", type: "link" });
  }

  for (let i = 0; i < items?.length; i++) {
    const item = items[i];
    if (item.SubMenus && item.SubMenus.length > 0) {
      const sectionItem = {
        text: item.Name,
        type: "section",
        defaultExpanded: false,
        items: MapMenuItemsToSideNavItems(item.SubMenus, false),
      } as SideNavigationProps.Item;

      results.push(sectionItem);
    } else {
      results.push({ text: item.Name, href: "#" + item.Id, type: "link" });
    }
  }

  return results;
};

const MapMenuItemToAppMenuItem = (
  items: MenuItem[],
  path: string = ""
): AppMenuItem[] => {
  if (!items) {
    return [];
  }
  return items.map((item) => ({
    path: path + "/" + item.Id,
    href: item.Id,
    id: item.Id,
    name: item.Name,
    parentMenu: null,
    subMenus: [
      {
        id: "#back",
        event: null,
        position: 0,
        canDelete: false,
        href: "#back/" + item.Id,
        name: "<<",
        parentMenu: null,
        path: "",
        subMenus: [],
      },
      ...MapMenuItemToAppMenuItem(item.SubMenus, path + "/" + item.Id),
    ],
    event: item.Event,
    position: item.Position,
    canDelete: item.CanDelete,
  }));
};

export const MenuProvider = ({ children }) => {
  const [appMenuItems, setAppMenuItems] = useState<AppMenuItem[]>([]);
  const [sideNavMenuItems, setSideNavMenuItems] = useState<
    SideNavigationProps.Item[]
  >([]);

  const [menuCache, setMenuCache] = useState<Record<number, MenuDetail[]>>({}); // Maybe i shouldn't cache? Some stuff should reload
  const [currentContentType, setCurrentContentType] =
    useState<AppLayoutProps.ContentType>("default");

  const [currentMenu, setCurrentMenu] = useState<MenuDetail>(null);

  const auth = useAuth();
  const api = useApi();
  const navigate = useNavigate();

  const location = useLocation();

  const loadMenus = async () => {
    const menuData = await api.makeApiCall<MenuItem[]>("getUserMenu", "GET");
    const menuItems = MapMenuItemToAppMenuItem(menuData, "");

    const sideNavItems = MapMenuItemsToSideNavItems(menuData);

    setAppMenuItems(menuItems);
    setSideNavMenuItems(sideNavItems);

    // check current path and simulate menu click
    const pathValues = location.pathname.split("/");
    console.log(pathValues);
    if (pathValues[pathValues.length - 1]) {
      const lastValue = pathValues[pathValues.length - 1];
      const lastValueNumber = Number(lastValue);
      if (isFinite(lastValueNumber)) {
        await onMenuClick(lastValueNumber);
      } else {
        console.warn(
          "current location path value is not a number",
          location.pathname
        );
      }
    }
  };

  const onHomeClick = async () => {
    navigate("/");
    setCurrentContentType("default");
  };

  const onMenuClick = async (event: number) => {
    //TODO: Need to show busy indicator
    // maybe... (works for now but don't like it)
    await onHomeClick();

    let menuDetails = menuCache[event];

    if (!menuDetails) {
      const url = "executeUIAction/" + event;

      menuDetails = await api.makeApiCall<MenuDetail[]>(url, "POST");

      const newCache = { ...menuCache };
      newCache[event] = menuDetails;
      setMenuCache(newCache);
    }

    for (let i = 0; i < menuDetails.length; i++) {
      const item = menuDetails[i];

      switch (item.ActionType) {
        case 0: {
          setCurrentContentType("table");
          setCurrentMenu(item);
          navigate("/view/" + event);
          break;
        }
        default:
          console.warn("Unknown action type: " + item.ActionType);
        // show global message?
      }

      break; // don't perform multiple actions for now, need to figure out how this will work
    }
  };

  useEffect(() => {
    if (auth.isAuthenticated === true) {
      loadMenus();
    } else {
      setAppMenuItems([]);
      setSideNavMenuItems([]);
    }
  }, [auth.isAuthenticated]);

  const value = {
    appMenuItems,
    sideNavMenuItems,
    onMenuClick,
    currentContentType,
    onHomeClick,
    currentMenu,
  };

  return <MenuContext.Provider value={value}>{children}</MenuContext.Provider>;
};

export const useMenu = () => {
  const context = useContext(MenuContext);
  if (!context) {
    throw new Error("useMenu must be used within an MenuProvider");
  }
  return context;
};